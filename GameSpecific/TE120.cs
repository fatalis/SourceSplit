using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TE120 : GameSupport
    {
        // start: when player view entity changes
        // ending: when player has lagged movement (speedmod)

        private bool _onceFlag;

        private int _camIndex;
        private MemoryWatcher<float> _playerLaggedMoveValue;
        private int _laggedMovementOffset = -1;

        public TE120()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "chapter_1";
            this.LastMap = "chapter_4";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _laggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _laggedMovementOffset.ToString("X"));
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._camIndex = state.GetEntIndexByName("blackout_viewcontrol");
                Debug.WriteLine("blackout_viewcontrol index is " + this._camIndex);
            }

            else if (this.IsLastMap && _laggedMovementOffset != -1)
            {
                _playerLaggedMoveValue = new MemoryWatcher<float>(state.PlayerEntInfo.EntityPtr + _laggedMovementOffset);
            }

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
            {
                return GameSupportResult.DoNothing;
            }

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex == this._camIndex
                    && state.PlayerViewEntityIndex == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("te120 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            // TODO: improve this to not rely on entity pointers
            else if (this.IsLastMap)
            {
                _playerLaggedMoveValue.Update(state.GameProcess);

                if (_playerLaggedMoveValue.Old == 1 && _playerLaggedMoveValue.Current == 0.3f)
                {
                    _onceFlag = true;
                    Debug.WriteLine("te120 end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

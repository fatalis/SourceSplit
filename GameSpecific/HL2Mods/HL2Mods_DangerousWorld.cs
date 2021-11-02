using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DangerousWorld : GameSupport
    {
        // start: when player's view index changes from the camera entity to the player
        // ending: when the final trigger_once is hit and the fade finishes

        private bool _onceFlag = false;
        private int _laggedMovementOffset = -1;
        private MemoryWatcher<float> _playerLaggedMoveValue;
        private float _splitTime;

        public HL2Mods_DangerousWorld()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("dw_ep1_01");
            this.AddLastMap("dw_ep1_08");
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _laggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _laggedMovementOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                _playerLaggedMoveValue = new MemoryWatcher<float>(state.PlayerEntInfo.EntityPtr + _laggedMovementOffset);
            else if (this.IsLastMap)
                _splitTime = state.FindOutputFireTime("sound_outro_amb_03", "PlaySound", "", 20);
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                _playerLaggedMoveValue.Update(state.GameProcess);
                if (_playerLaggedMoveValue.Current == 1 && _playerLaggedMoveValue.Old != 1)
                {
                    Debug.WriteLine("dangerous world start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("sound_outro_amb_03", "PlaySound", "", 20);
                try
                {
                    if (splitTime != 0 && _splitTime == 0)
                    {
                        _onceFlag = true;
                        Debug.WriteLine("dangerous world end");
                        return GameSupportResult.PlayerLostControl;
                    }
                }
                finally { _splitTime = splitTime; }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

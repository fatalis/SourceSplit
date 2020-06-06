using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalTFV : GameSupport
    {
        // how to match this timing with demos:
        // start: crosshair appear
        // ending: crosshair disappear

        private int _laggedMovementOffset = -1;
        private bool _onceFlag;
        private IntPtr _gunshipMakerPtr;
        private const int VAULT_SAVE_TICK = 4261;

        public PortalTFV()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AutoStartType = AutoStart.ViewEntityChanged;
            this.FirstMap = "portaltfv1";
            this.LastMap = "portaltfv5";
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

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _laggedMovementOffset != -1)
            {
                _gunshipMakerPtr = state.GetEntityByName("gunshipmaker");
                Debug.WriteLine("Gunship Maker pointer = 0x" + _gunshipMakerPtr.ToString("X"));
            }

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            //if (this.IsFirstMap)
            //{
            //    // vault save starts at tick 4261, but update interval may miss it so be a little lenient
            //    if ((state.TickBase >= VAULT_SAVE_TICK && state.TickBase <= VAULT_SAVE_TICK+4) && !_onceFlag)
            //    {
            //        _onceFlag = true;
            //        int ticksSinceVaultSaveTick = state.TickBase - VAULT_SAVE_TICK; // account for missing ticks if update interval missed it
            //        this.StartOffsetTicks = -3534 - ticksSinceVaultSaveTick; // 53.01 seconds
            //        return GameSupportResult.PlayerGainedControl;
            //    }

            //    this.StartOffsetTicks = 1;
            //    return base.OnUpdate(state);
            //}
            //else if (!this.IsLastMap || _onceFlag)
            //    return GameSupportResult.DoNothing;

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                float laggedMovementValue;
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _laggedMovementOffset, out laggedMovementValue);
                if (laggedMovementValue==0.4f)
                {
                    Debug.WriteLine("tfv end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

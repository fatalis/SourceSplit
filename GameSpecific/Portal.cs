using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Portal : GameSupport
    {
        // how to match this timing with demos:
        // start: crosshair appear
        // ending: crosshair disappear

        private int _playerSuppressingCrosshairOffset = -1;
        private bool _prevCrosshairSuppressed;
        private bool _onceFlag;

        public Portal()
        {
            this.AutoStartType = AutoStart.ViewEntityChanged;
            this.FirstMap = "testchmb_a_00";
            this.LastMap = "escape_02";
            // match portal demo timer
            this.StartOffsetTicks = 1;
            this.EndOffsetTicks = -1;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_bSuppressingCrosshair", state.GameProcess, scanner, out _playerSuppressingCrosshairOffset))
                Debug.WriteLine("CPortalPlayer::m_bSuppressingCrosshair offset = 0x" + _playerSuppressingCrosshairOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _playerSuppressingCrosshairOffset != -1)
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _playerSuppressingCrosshairOffset, out _prevCrosshairSuppressed);

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (this.IsFirstMap)
                return base.OnUpdate(state);
            else if (!this.IsLastMap || _onceFlag)
                return GameSupportResult.DoNothing;

            if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _playerSuppressingCrosshairOffset != -1)
            {
                bool crosshairSuppressed;
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _playerSuppressingCrosshairOffset, out crosshairSuppressed);

                if (crosshairSuppressed && !_prevCrosshairSuppressed)
                {
                    _onceFlag = true;
                    Debug.WriteLine("porto crosshair detected");
                    return GameSupportResult.PlayerLostControl;
                }

                _prevCrosshairSuppressed = crosshairSuppressed;                
            }

            return GameSupportResult.DoNothing;
        }
    }
}

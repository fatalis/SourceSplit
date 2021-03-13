using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_PRMO : GameSupport
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: crosshair disappear

        private MemoryWatcher<bool> _crosshairSuppressed;
        private int _playerSuppressingCrosshairOffset = -1;
        private bool _onceFlag;

        public PortalMods_PRMO()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AutoStartType = AutoStart.ViewEntityChanged;
            this.FirstMap = "escape_02_d";
            this.LastMap = "testchmb_a_00_d";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
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
                _crosshairSuppressed = new MemoryWatcher<bool>(state.PlayerEntInfo.EntityPtr + _playerSuppressingCrosshairOffset);

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                _crosshairSuppressed.Update(state.GameProcess);

                if (!_crosshairSuppressed.Old && _crosshairSuppressed.Current)
                {
                    _onceFlag = true;
                    Debug.WriteLine("porto crosshair detected");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

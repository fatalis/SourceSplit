using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Synergy : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // xen start: when view entity changes back to the player's
        // ending: first tick nihilanth's health is zero
        // earthbound ending: when view entity changes to the ending camera's

        private bool _onceFlag;

        private StringWatcher _command;
        private CustomCommand _autosplitIL = new CustomCommand("ilstart");
        private List<string> _startMaps = new List<string>() { "d1_trainstation_01", "ep1_citadel_00", "ep2_outland_01" };
        private List<CustomCommand> _cmdList;

        public Synergy()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            _cmdList = new List<CustomCommand>( new CustomCommand[] { _autosplitIL });
        }

        public override void OnGameAttached(GameState state)
        {
            base.OnGameAttached(state);

            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            var commandTarg = new SigScanTarget(16, "55 8B EC 8D 45 ?? 50 FF 75 ?? 68 00 04 00 00 68 ?? ?? ?? ??");

            IntPtr commandPtr = state.GameProcess.ReadPointer(scanner.Scan(commandTarg));
            if (commandPtr != IntPtr.Zero)
                Debug.WriteLine("Command ptr found at 0x" + commandPtr.ToString("X"));
            else
                Debug.WriteLine("Command ptr not found!");

            _command = new StringWatcher(commandPtr + 0x11, 20);
            HandleInputCommand(state, true);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
        }

        void HandleInputCommand(GameState state, bool ignoreChanged = false)
        {
            _command.Update(state.GameProcess);
            if (ignoreChanged || _command.Changed)
            {
                if (string.IsNullOrEmpty(_command.Current))
                    return;

                string cleanedCmd = _command.Current.Replace("\n", "").Replace("\r", "").ToLower();

                foreach (CustomCommand cmd in _cmdList)
                {
                    if (cleanedCmd.Contains(cmd.Name) && cleanedCmd.Length > cmd.Name.Length)
                    {
                        string arg = cleanedCmd.Substring(cleanedCmd.IndexOf(cmd.Name) + cmd.Name.Length, 1);
                        SystemSounds.Asterisk.Play();
                        cmd.Update(arg != "0");
                    }
                }
            }
        }

        public override void OnGenericUpdate(GameState state)
        {
            if (_autosplitIL.Enabled)
            {
                if (!StartOnFirstLoadMaps.Contains(state.CurrentMap))
                {
                    StartOnFirstLoadMaps.Clear();
                    StartOnFirstLoadMaps.Add(state.CurrentMap);
                }
            }
            else
                StartOnFirstLoadMaps.Clear();
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            HandleInputCommand(state);

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            return GameSupportResult.DoNothing;
        }
    }
}
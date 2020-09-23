using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;
using System.Media;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class BMSRetail : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // xen start: when view entity changes back to the player's
        // ending: first tick nihilanth's health is zero
        // earthbound ending: when view entity changes to the ending camera's

        private bool _onceFlag;

        private int _baseEntityHealthOffset = -1;
        private const int _serverModernModuleSize = 0x9D6000;
        private const int _serverModModuleSize = 0x81B000;

        private StringWatcher _command;
        private bool _handleInputCommandEnabled = true;
        private bool _ebEnd = false;
        private string _ebEndMap = "bm_c3a2i";
        private bool _xenStart = false;

        private MemoryWatcher<int> _nihiHP;
        private int _ebCamIndex;
        private int _xenCamIndex;

        public BMSRetail()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.StartOnFirstMapLoad = true;
            this.FirstMap = "bm_c1a0a";
            this.LastMap = "bm_c4a4a";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            var commandTarg = new SigScanTarget(16, "55 8B EC 8D 45 ?? 50 FF 75 ?? 68 00 04 00 00 68 ?? ?? ?? ??");

            IntPtr commandPtr = state.GameProcess.ReadPointer(scanner.Scan(commandTarg));
            if (commandPtr != IntPtr.Zero)
                Debug.WriteLine("Command ptr found at 0x" + commandPtr.ToString("X"));
            else
                Debug.WriteLine("Command ptr not found!");

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));


            _handleInputCommandEnabled = true;
            // for versions before .91, disable handleinputcommand as it's redundant
            if (server.ModuleMemorySize < _serverModernModuleSize)
            {
                _ebEnd = true;
                _handleInputCommandEnabled = false;
                // for mod, eb's final map name is different
                if (server.ModuleMemorySize <= _serverModModuleSize)
                    _ebEndMap = "bm_c3a2h";
            }

            if (_handleInputCommandEnabled)
            {
                _command = new StringWatcher(commandPtr + 0x11, 9);

                // if livesplit is loaded after the player has put in a valid command, then check
                HandleInputCommand(state, true);
            }
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            _onceFlag = false;

            if (state.CurrentMap.ToLower() == _ebEndMap)
            {
                _ebCamIndex = state.GetEntIndexByName("locked_in");
            }

            if (state.CurrentMap.ToLower() == "bm_c4a1a")
            {
                _xenCamIndex = state.GetEntIndexByName("stand_viewcontrol");
            }

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                IntPtr nihiPtr = state.GetEntityByName("nihilanth");
                Debug.WriteLine("Nihilanth pointer = 0x" + nihiPtr.ToString("X"));
                _nihiHP = new MemoryWatcher<int>(nihiPtr + _baseEntityHealthOffset);
            }
        }


        // aside from full game, bms also has earthbound and xen only, whose start and end conflict those of 
        // normal full game, so we'll let the runner choose whether we'll start or stop through tracking console commands
        // search the command buffer if the runner has put in any commands
        // this command buffer only responses to unknown commands in terms of user-inputted ones from the console

        // format: ebend<arg>, xenstart<arg>, eg: ebend1, xenstart0, characters are also accepted which will be interpreted as true
        public void HandleArg(string command, string name, ref bool target)
        {
            string arg = command.Substring(command.Length - 1, 1);
            if (arg != "0") target = true;
            else target = false;

            Debug.WriteLine(name + " is " + ((arg != "0") ? "Enabled" : "Disabled"));

            // play the warning sound to let people know its toggled
            SystemSounds.Asterisk.Play();
        }

        public bool CheckCommand(string cmd, string targetCmd)
        {
            return cmd.Length - 1 == (targetCmd).Length && cmd.Substring(0, cmd.Length - 1) == targetCmd;
        }

        public void HandleInputCommand(GameState state, bool ignoreChanged = false)
        {
            if (!_handleInputCommandEnabled)
                return;

            _command.Update(state.GameProcess);
            if (ignoreChanged || _command.Changed)
            {
                // remove any carriage returns
                string cleanedCmd = _command.Current.Replace("\n", "").Replace("\r", "").ToLower();
                if (CheckCommand(cleanedCmd, "ebend"))
                {
                    HandleArg(cleanedCmd, "Earthbound Auto-end", ref _ebEnd);
                }
                else if (CheckCommand(cleanedCmd, "xenstart"))
                {
                    HandleArg(cleanedCmd, "Xen Auto-start", ref _xenStart);
                }
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            HandleInputCommand(state);

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                _nihiHP.Update(state.GameProcess);
                if (_nihiHP.Current <= 0 && _nihiHP.Old > 0)
                {
                    Debug.WriteLine("black mesa end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            else if (_ebEnd && state.CurrentMap.ToLower() == _ebEndMap)
            {
                if (state.PlayerViewEntityIndex == _ebCamIndex && state.PrevPlayerViewEntityIndex == 1)
                {
                    Debug.WriteLine("bms eb end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            else if (_xenStart && state.CurrentMap.ToLower() == "bm_c4a1a")
            {
                if (state.PlayerViewEntityIndex == 1 && state.PrevPlayerViewEntityIndex == _xenCamIndex)
                {
                    Debug.WriteLine("bms xen start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
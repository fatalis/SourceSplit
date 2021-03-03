using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Ep2 : GameSupport
    {
        // how to match this timing with demos:
        // start: 
        // ending: the tick where velocity changes from 600.X to 0.0 AFTER the camera effects (cl_showpos 1)

        // mods:
            // dark intervention, hells mines, upmine struggle:
                // start: on map load
                // end: when final output to exit the map is fired

        private bool _onceFlag;
        private int _basePlayerLaggedMovementOffset = -1;
        private float _prevLaggedMovementValue;
        private float _splitTime;

        public HL2Ep2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ep2_outland_01";
            this.LastMap = "ep2_outland_12a";
            this.StartOnFirstLoadMaps.AddRange(new string[] { "Dark_intervention", "hells_mines", "twhl_upmine_struggle" });
            this.RequiredProperties = PlayerProperties.ParentEntity;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _basePlayerLaggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _basePlayerLaggedMovementOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _basePlayerLaggedMovementOffset != -1)
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _basePlayerLaggedMovementOffset, out _prevLaggedMovementValue);

            _onceFlag = false;
            _splitTime = 0f;
        }

        public override void OnGenericUpdate(GameState state)
        {
            if (state.HostState == HostState.GameShutdown)
                this.OnUpdate(state);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            switch (state.CurrentMap.ToLower())
            {
                default:
                    {
                        if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _basePlayerLaggedMovementOffset != -1)
                        {
                            // "OnMapSpawn" "startcar_speedmod,ModifySpeed,0,0,-1"
                            // "OnMapSpawn" "startcar_speedmod,ModifySpeed,1,12.5,-1"

                            float laggedMovementValue;
                            state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _basePlayerLaggedMovementOffset, out laggedMovementValue);

                            if (laggedMovementValue.BitEquals(1.0f) && !_prevLaggedMovementValue.BitEquals(1.0f))
                            {
                                Debug.WriteLine("ep2 start");
                                _onceFlag = true;
                                return GameSupportResult.PlayerGainedControl;
                            }

                            _prevLaggedMovementValue = laggedMovementValue;
                        }
                        else if (this.IsLastMap)
                        {
                            // "OnTrigger4" "cvehicle.hangar,EnterVehicle,,0,1"

                            if (state.PlayerParentEntityHandle != -1
                                && state.PrevPlayerParentEntityHandle == -1)
                            {
                                Debug.WriteLine("ep2 end");
                                _onceFlag = true;
                                this.EndOffsetTicks = 0;
                                return GameSupportResult.PlayerLostControl;
                            }
                        }
                        break;
                    }
                case "dark_intervention":
                    {
                        float splitTime = state.FindOutputFireTime("command_ending", 3);
                        _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                        if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                        {
                            Debug.WriteLine("dark intervention end");
                            _splitTime = 0f;
                            _onceFlag = true;
                            SplitOnNextSessionEnd = true;
                        }
                        break;
                    }
                case "hells_mines":
                    {
                        float splitTime = state.FindOutputFireTime("command", 3);
                        _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                        if (state.CompareToInternalTimer(_splitTime, 0f, false , true))
                        {
                            _splitTime = 0f;
                            _onceFlag = true;
                            SplitOnNextSessionEnd = true;
                        }
                        break;
                    }
                case "twhl_upmine_struggle":
                    {
                        float splitTime = state.FindOutputFireTime("command", 3);

                        _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                        if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                        {
                            Debug.WriteLine("upmine struggle end");
                            _splitTime = 0f;
                            _onceFlag = true;
                            SplitOnNextSessionEnd = true;
                        }
                        break;
                    }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

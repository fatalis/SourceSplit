// what is this?
// to cut down on the number of files included in this project, this file was created
// the mods included in this file have either no or similar splitting behavior (start on map load, end on game disconnect)

// mods included: think tank, gnome, hl2 backwards mod, hl2 reject, trapville, rtslville, 
// hl abridged, episode one, combination ville, phaseville, companion piece, school adventures, the citizen 1
// hells mines, dark intervention, upmine struggle, offshore

using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Misc : GameSupport
    {
        internal float _splitTime;
        internal bool _onceFlag;

        public override void OnGenericUpdate(GameState state)
        {
            if (IsLastMap && state.HostState == HostState.GameShutdown)
                OnUpdate(state);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            return base.OnUpdate(state);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
            _splitTime = 0f;
        }
    }

    class HL2Mods_ThinkTank : HL2Mods_Misc
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final output is fired

        public HL2Mods_ThinkTank()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("ml04_ascend");
            this.AddLastMap("ml04_crown_bonus");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("servercommand", 3);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    _onceFlag = true;
                    Debug.WriteLine("think tank end");
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_Gnome : HL2Mods_Misc
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final output is fired

        public HL2Mods_Gnome()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("at03_findthegnome");
            this.AddLastMap("at03_nev_no_gnomes_land");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("cmd_end", 2);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("gnome end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_BackwardsMod : GameSupport
    {
        // start: on first map
        public HL2Mods_BackwardsMod()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("backward_d3_breen_01");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }
    }

    class HL2Mods_Reject : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_Reject()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.StartOnFirstLoadMaps.Add("reject");
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            float splitTime = state.FindOutputFireTime("komenda", 3);
            _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
            if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
            {
                Debug.WriteLine("hl2 reject end");
                _onceFlag = true;
                state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_TrapVille : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_TrapVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("aquickdrivethrough_thc16c4");
            this.AddLastMap("makeearthgreatagain_thc16c4");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }


        private Vector3f _endSector = new Vector3f(7953f, -11413f, 2515f);

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            // todo: probably should use the helicopter's position?
            if (IsLastMap && state.PlayerPosition.Distance(_endSector) <= 300f)
            {
                float splitTime = state.FindOutputFireTime("game_end", 10);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("trapville end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_RTSLVille : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_RTSLVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("from_ashes_map1_rtslv");
            this.AddLastMap("terminal_rtslv");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap && state.PlayerViewEntityIndex != GameState.ENT_INDEX_PLAYER)
            {
                float splitTime = state.FindOutputFireTime("clientcommand", 8);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("rtslville end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_Abridged : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_Abridged()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("ml05_training_facilitea");
            this.AddLastMap("ml05_shortcut17");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("end_disconnect", "command", "disconnect; map_background background_ml05", 6);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("hl abridged end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_EpisodeOne : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_EpisodeOne()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("direwolf");
            this.AddLastMap("outland_resistance");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("point_clientcommand2", 4);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("episode one end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_CombinationVille : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_CombinationVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("canal_flight_ppmc_cv");
            this.AddLastMap("cvbonus_ppmc_cv");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        private Vector3f _tramEndPos = new Vector3f(2624f, -1856f, 250f);
        private int _tramPtr;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsLastMap)
                _tramPtr = state.GetEntIndexByName("tram");

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap && state.GetEntityPos(_tramPtr).Distance(_tramEndPos) <= 100)
            {
                float splitTime = state.FindOutputFireTime("pcc", "command", "startupmenu force", 8);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("combination ville end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_PhaseVille : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_PhaseVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("rtsl_mlc");
            this.AddLastMap("hospitalisation_tlc18_c4");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("clientcommand", 3);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("phaseville end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_CompanionPiece : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_CompanionPiece()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("tg_wrd_carnival");
            this.AddLastMap("maplab_jan_cp");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("piss_off_egg_head", 4);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("companion piece end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_TheCitizen : GameSupport
    {
        // start: on first map
        public HL2Mods_TheCitizen()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("thecitizen_part1");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }
    }

    class HL2Mods_SchoolAdventures : HL2Mods_Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public HL2Mods_SchoolAdventures()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("sa_01");
            this.AddLastMap("sa_04");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        private int _endCameraIndex = -1;

        public override void OnGenericUpdate(GameState state) { }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsLastMap)
            {
                _endCameraIndex = state.GetEntIndexByName("viewcontrol_credits");
                Debug.WriteLine($"Found end camera index at {_endCameraIndex}");
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap && _endCameraIndex != -1)
            {
                if (state.PrevPlayerViewEntityIndex == 1 &&
                    state.PlayerViewEntityIndex == _endCameraIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("school_adventures end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_DarkIntervention : HL2Mods_Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public HL2Mods_DarkIntervention()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("Dark_intervention");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("command_ending", 3);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("dark intervention end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_HellsMines : HL2Mods_Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public HL2Mods_HellsMines()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("hells_mines");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("command", 3);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("hells mines end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_UpmineStruggle : HL2Mods_Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public HL2Mods_UpmineStruggle()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("twhl_upmine_struggle");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("command", 3);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    Debug.WriteLine("upmine struggle end");
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_Offshore : HL2Mods_Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: on game disconnect after final output has been fired.

        public HL2Mods_Offshore()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("islandescape");
            this.AddLastMap("islandcitytrain");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("launchQuit", 5);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
                {
                    _onceFlag = true;
                    state.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

// what is this?
// to cut down on the number of files included in this project, this file was created
// the mods included in this file have either no or similar splitting behavior (start on map load, end on game disconnect)

// mods included: think tank, gnome, hl2 backwards mod, hl2 reject, trapville, rtslville, 
// hl abridged, episode one, combination ville, phaseville, companion piece, school adventures, the citizen 1

using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;


namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_ThinkTank : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final output is fired

        private bool _onceFlag;

        public HL2Mods_ThinkTank()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ml04_ascend";
            this.LastMap = "ml04_crown_bonus";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
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
                if (state.CompareToInternalTimer(splitTime))
                {
                    _onceFlag = true;
                    Debug.WriteLine("think tank end");
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        } 
    }

    class HL2Mods_Gnome : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final output is fired

        private bool _onceFlag;

        public HL2Mods_Gnome()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "at03_findthegnome";
            this.LastMap = "at03_nev_no_gnomes_land";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
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
                if (state.CompareToInternalTimer(splitTime))
                {
                    Debug.WriteLine("gnome end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "backward_d3_breen_01";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }
    }

    class HL2Mods_Reject : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_Reject()
        { 
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "reject";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        private bool _onceFlag;

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            float splitTime = state.FindOutputFireTime("komenda", 3);
            if (state.CompareToInternalTimer(splitTime))
            {
                Debug.WriteLine("hl2 reject end");
                _onceFlag = true;
                this.EndOffsetTicks = -1;
                return GameSupportResult.PlayerLostControl;
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_TrapVille : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_TrapVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "aquickdrivethrough_thc16c4";
            this.LastMap = "makeearthgreatagain_thc16c4";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        private bool _onceFlag = false;
        private Vector3f _endSector = new Vector3f(7953f, -11413f, 2515f);

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            // todo: probably should use the helicopter's position?
            if (IsLastMap && state.PlayerPosition.Distance(_endSector) <= 300f)
            {
                float splitTime = state.FindOutputFireTime("game_end", 10);
                if (state.CompareToInternalTimer(splitTime))
                {
                    Debug.WriteLine("trapville end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_RTSLVille : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_RTSLVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "from_ashes_map1_rtslv";
            this.LastMap = "terminal_rtslv";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        private bool _onceFlag = false;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap && state.PlayerViewEntityIndex != GameState.ENT_INDEX_PLAYER)
            {
                float splitTime = state.FindOutputFireTime("clientcommand", 8);
                if (state.CompareToInternalTimer(splitTime))
                {
                    Debug.WriteLine("rtslville end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_Abridged : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_Abridged()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ml05_training_facilitea";
            this.LastMap = "ml05_shortcut17";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        private bool _onceFlag = false;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("end_disconnect", "command", "disconnect; map_background background_ml05", 6);
                if (state.CompareToInternalTimer(splitTime))
                {
                    Debug.WriteLine("hl abridged end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_EpisodeOne : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_EpisodeOne()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "direwolf";
            this.LastMap = "outland_resistance";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        private bool _onceFlag = false;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("point_clientcommand2", 4);
                if (state.CompareToInternalTimer(splitTime))
                {
                    Debug.WriteLine("episode one end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_CombinationVille : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_CombinationVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "canal_flight_ppmc_cv";
            this.LastMap = "cvbonus_ppmc_cv";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        private bool _onceFlag = false;

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
                if (state.CompareToInternalTimer(splitTime))
                {
                    Debug.WriteLine("combination ville end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_PhaseVille : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_PhaseVille()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "rtsl_mlc";
            this.LastMap = "hospitalisation_tlc18_c4";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        private bool _onceFlag = false;


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("clientcommand", 3);
                if (state.CompareToInternalTimer(splitTime))
                {
                    Debug.WriteLine("phaseville end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }

    class HL2Mods_CompanionPiece : GameSupport
    {
        // start: on first map
        // end: on final output
        public HL2Mods_CompanionPiece()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "tg_wrd_carnival";
            this.LastMap = "maplab_jan_cp";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        private bool _onceFlag = false;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("piss_off_egg_head", 4);
                if (state.CompareToInternalTimer(splitTime))
                {
                    Debug.WriteLine("companion piece end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "thecitizen_part1";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }
    }

    class HL2Mods_SchoolAdventures : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        private bool _onceFlag;

        public HL2Mods_SchoolAdventures()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "sa_01";
            this.LastMap = "sa_04";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
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
                float splitTime = state.FindOutputFireTime("final_teleport1", 3);
                if (state.CompareToInternalTimer(splitTime))
                {
                    _onceFlag = true;
                    Debug.WriteLine("school_adventures end");
                    this.EndOffsetTicks = -1;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

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
        public float SplitTime;
        public bool OnceFlag;

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
            OnceFlag = false;
            SplitTime = 0f;
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
            this.FirstMap = "ml04_ascend";
            this.LastMap = "ml04_crown_bonus";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            OnceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("servercommand", 3);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    OnceFlag = true;
                    Debug.WriteLine("think tank end");
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "at03_findthegnome";
            this.LastMap = "at03_nev_no_gnomes_land";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            OnceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("cmd_end", 2);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("gnome end");
                    OnceFlag = true;
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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

    class HL2Mods_Reject : HL2Mods_Misc
    {
        // start: on first map
        // end: on final output
        public HL2Mods_Reject()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.LastMap = "reject";
            this.StartOnFirstLoadMaps.Add(this.LastMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            float splitTime = state.FindOutputFireTime("komenda", 3);
            SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
            if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
            {
                Debug.WriteLine("hl2 reject end");
                OnceFlag = true;
                QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "aquickdrivethrough_thc16c4";
            this.LastMap = "makeearthgreatagain_thc16c4";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }


        private Vector3f _endSector = new Vector3f(7953f, -11413f, 2515f);

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            // todo: probably should use the helicopter's position?
            if (IsLastMap && state.PlayerPosition.Distance(_endSector) <= 300f)
            {
                float splitTime = state.FindOutputFireTime("game_end", 10);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("trapville end");
                    OnceFlag = true;
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "from_ashes_map1_rtslv";
            this.LastMap = "terminal_rtslv";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap && state.PlayerViewEntityIndex != GameState.ENT_INDEX_PLAYER)
            {
                float splitTime = state.FindOutputFireTime("clientcommand", 8);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("rtslville end");
                    OnceFlag = true;
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "ml05_training_facilitea";
            this.LastMap = "ml05_shortcut17";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("end_disconnect", "command", "disconnect; map_background background_ml05", 6);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("hl abridged end");
                    OnceFlag = true;
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "direwolf";
            this.LastMap = "outland_resistance";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("point_clientcommand2", 4);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("episode one end");
                    OnceFlag = true;
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "canal_flight_ppmc_cv";
            this.LastMap = "cvbonus_ppmc_cv";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        private Vector3f _tramEndPos = new Vector3f(2624f, -1856f, 250f);
        private int _tramPtr;

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsLastMap)
                _tramPtr = state.GetEntIndexByName("tram");

            OnceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap && state.GetEntityPos(_tramPtr).Distance(_tramEndPos) <= 100)
            {
                float splitTime = state.FindOutputFireTime("pcc", "command", "startupmenu force", 8);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("combination ville end");
                    OnceFlag = true;
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "rtsl_mlc";
            this.LastMap = "hospitalisation_tlc18_c4";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("clientcommand", 3);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("phaseville end");
                    OnceFlag = true;
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "tg_wrd_carnival";
            this.LastMap = "maplab_jan_cp";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("piss_off_egg_head", 4);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("companion piece end");
                    OnceFlag = true;
                    QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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

    class HL2Mods_SchoolAdventures : HL2Mods_Misc
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        public HL2Mods_SchoolAdventures()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "sa_01";
            this.LastMap = "sa_04";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnGenericUpdate(GameState state) { }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("final_teleport1", 3);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, true))
                {
                    OnceFlag = true;
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
            this.FirstMap = "Dark_intervention";
            this.LastMap = this.FirstMap;
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("command_ending", 3);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("dark intervention end");
                    OnceFlag = true;
                    this.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "hells_mines";
            this.LastMap = this.FirstMap;
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("command", 3);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("hells mines end");
                    OnceFlag = true;
                    this.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "twhl_upmine_struggle";
            this.LastMap = this.FirstMap;
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("command", 3);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    Debug.WriteLine("upmine struggle end");
                    OnceFlag = true;
                    this.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
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
            this.FirstMap = "islandescape";
            this.LastMap = "islandcitytrain";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            OnceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (OnceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("launchQuit", 5);
                SplitTime = (splitTime == 0f) ? SplitTime : splitTime;
                if (state.CompareToInternalTimer(SplitTime, 0f, false, true))
                {
                    OnceFlag = true;
                    this.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

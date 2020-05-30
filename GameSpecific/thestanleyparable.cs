using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;


namespace LiveSplit.SourceSplit.GameSpecific
{
    class thestanleyparable : GameSupport
    {
        // start: when player first moves OR their view angle first changes OR when they close the door while standing still AND they mustve been teleported

        // endings:
        // freedom:     when player's parent entity handle changes from nothing
        // countdown:   when the player's view entity changes to the final whiteout camera
        // art:         when the player's view entity changes to the final camera
        // reluctant:   when the player's view entity changes to the final blackout camera AND the player is within 10 units of the 427 room
        // escape:      when the player's view entity changes to the final camera
        // broom:       when the player is within 50 units of the center of the room
        // choice:      when stanley_drawcredits is set to 1
        // confusion:   when the cmd point_clientcommand entity gets fed "tsp_reload 5", this only gets detected in onsessionend, the split happens on map1
        // games:       when the player's view entity changes to the final blackout camera
        // heaven:      when the tsp_buttonpass counter is 4 or higher AND it is increased when the final button in stanley's room is pressed
        // insane:      when the player's view entity changes to the final blackout camera AND the player is wihtin the rooms before the cutscene
        // museum:      when the cmd point_clientcommand entity gets fed "StopSound"
        // serious:     when you've visited the seriousroom map 3 times
        // disco:       when the y axis speed of the entity that rotates the text is 20
        // stuck:       when the cube's bounding box touches the trigger (done with math, hopefully not too expensive)
        // song:        2000 ticks after pressing the button
        // voice over:  6 ticks after pressing the button
        // space:       when the previous player's X pos is less than -7000 and current is higher than -400
        // zending:     when the player view entity switches from the player to the final camera
        // whiteboard:  when the previous player's X pos is =< 1993 and current is >= 1993

        private bool _onceFlag;
        public static bool _resetFlag;
        public static bool _resetFlag2;

        ProcessModuleWow64Safe client;
        ProcessModuleWow64Safe engine;
        ProcessModuleWow64Safe server;

        // offsets
        private int _baseEntityAngleOffset = -1;
        private int _baseEntityAngleVelOffset = -1;
        private const int angleoffset = 2812;
        private const int brushdisabledoffset = 864;
        private const int buttonlockoffset = 1040;
        private const int drawcreditsoffset = 16105320;
        private const int clientcommandinputoffset = 6726216;
        private const int buttonpasses = 8263812;
        private int credits;
        IntPtr commandptr;


        // endings
        //Vector3f oldpos = new Vector3f(-154.778f, -205.209f, 0f); TODO: add 2nd start position which is where you end up after the intro cutscene
        Vector3f startpos = new Vector3f(-200f, -208f, 0.03125f);
        Vector3f startang = new Vector3f(0f, 90f, 0f);
        Vector3f spawnpos = new Vector3f(5152f, 776f, 3328f);
        Vector3f doorstartang = new Vector3f(0f, 360f, 0f);
        private IntPtr ending_song_brush;
        private int ending_song_disabled;
        private static bool ending_song_oktoconsider;
        private int ending_song_trig_index;
        private IntPtr ending_vo_button_index;
        private int ending_vo_button_locked;
        private Vector3f ending_outside_region = new Vector3f(-704f, 256f, -256f);
        private int ending_heaven_br_index;
        private int ending_heaven_bpass1;
        private int ending_escape_cam_index;
        private int ending_map1_cam_index;
        private int ending_count_cam_index;
        private int ending_art_cam_index;
        private int ending_games_cam_index;
        Vector3f ending_broom_trig_origin = new Vector3f(-592f, 2208f, 64f);
        Vector3f ending_insane_sector_origin = new Vector3f(-6072f, 888f, 0);
        private int ending_insane_cam_index;
        private static int ending_serious_count;
        private int ending_disco_text_index;
        Vector3f ending_disco_angvel;
        private int ending_stuck_box_index;
        private int ending_stuck_box_index_2;
        private int ending_zending_cam_index;
        Vector3f ending_whiteboard_door_origin = new Vector3f(1988f, -1792f, -1992f);
        private static bool ending_confuse_flag = false;

        public static bool stanley;

        public thestanleyparable()
        {
            stanley = true;
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            //this.FirstMap = "map1"; <= we go back to this map a lot so its better to just disable this
            //this.LastMap = NAH;
            this.RequiredProperties = PlayerProperties.ViewEntity | PlayerProperties.Position | PlayerProperties.ParentEntity;
        }

        public static void resetflag()
        {
            _resetFlag = false;
            _resetFlag2 = false;
            ending_serious_count = 0;
            ending_song_oktoconsider = false;
            ending_confuse_flag = false;
        }

        public static void resetflag2()
        {
            _resetFlag = true;
        }

        public override void OnGameAttached(GameState state)
        {
            server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            client = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "client.dll");
            engine = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "engine.dll");

            Trace.Assert(server != null && client != null && engine != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_angAbsRotation", state.GameProcess, scanner, out _baseEntityAngleOffset))
                Debug.WriteLine("CBaseEntity::m_angAbsRotation offset = 0x" + _baseEntityAngleOffset.ToString("X"));

            if (GameMemory.GetBaseEntityMemberOffset("m_vecAngVelocity", state.GameProcess, scanner, out _baseEntityAngleVelOffset))
                Debug.WriteLine("CBaseEntity::m_vecAngVelocity offset = 0x" + _baseEntityAngleVelOffset.ToString("X"));

            ending_serious_count = 0;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            switch (state.CurrentMap.ToLower())
            {
                case "map1":
                    {
                        this.ending_map1_cam_index = state.GetEntIndexByName("cam_black");
                        this.ending_heaven_br_index = state.GetEntIndexByName("427compbr");
                        state.GameProcess.ReadValue(server.BaseAddress + buttonpasses, out ending_heaven_bpass1);
                        this.ending_insane_cam_index = state.GetEntIndexByName("mariella_camera");
                        this.ending_song_brush = state.GetEntityByName("errorgag");
                        this.ending_song_trig_index = state.GetEntIndexByPos(-620f, 256f, -208f);
                        this.ending_vo_button_index = state.GetEntInfoByIndex(state.GetEntIndexByPos(-703f, 295f, -198f)).EntityPtr;

                        state.GameProcess.ReadValue(ending_song_brush + brushdisabledoffset, out ending_song_disabled);
                        if ((state.GetEntInfoByIndex(ending_song_trig_index).EntityPtr == IntPtr.Zero || ending_song_trig_index == -1) && ending_song_disabled == 0)
                        {
                            SourceSplitComponent.ResetSpecialSplit();
                        }
                        break;
                    }
                case "map2":
                    {
                        this.ending_count_cam_index = state.GetEntIndexByName("cam_white");
                        this.ending_disco_text_index = state.GetEntIndexByName("emotionboothDrot");
                        break;
                    }
                case "redstair":
                    {
                        this.ending_escape_cam_index = state.GetEntIndexByName("blackcam");
                        break;
                    }
                case "babygame":
                    {
                        this.ending_art_cam_index = state.GetEntIndexByName("whitecamera");
                        break;
                    }
                case "map_death":
                case "map":
                    {
                        commandptr = engine.BaseAddress + clientcommandinputoffset;
                        break;
                    }
                case "testchmb_a_00":
                    {
                        this.ending_games_cam_index = state.GetEntIndexByName("blackoutend");
                        ending_stuck_box_index_2 = state.GetEntIndexByName("box");
                        break;
                    }
                case "seriousroom":
                    {
                        ending_serious_count += 1;
                        break;
                    }
                case "zending":
                    {
                        ending_zending_cam_index = state.GetEntIndexByName("cam_dead");
                        break;
                    }
            }

            _onceFlag = false;
        }

        public GameSupportResult defaultend(string endingname)
        {
            _onceFlag = true;
            Debug.WriteLine(endingname + " ending");
            return GameSupportResult.PlayerLostControl;
        }

        public override void OnSessionEnd(GameState state)
        {
            if (state.CurrentMap.ToLower() == "map") // confusion ending
            {
                string cmd2;
                state.GameProcess.ReadString(commandptr, ReadStringType.ASCII, 12, out cmd2);
                if (cmd2 == "tsp_reload 5")
                {
                    ending_confuse_flag = true;
                }
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;


            switch (state.CurrentMap.ToLower())
            {
                default:
                    {
                        return GameSupportResult.DoNothing;
                    }

                case "map1": // beginning, death, reluctant, whiteboard, broom closet, song, voice over, cold feet, insane, apartment, heaven ending
                    {

                        if (_resetFlag == false)
                        {
                            Vector3f ang;
                            Vector3f doorang;

                            state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + angleoffset, out ang);
                            state.GameProcess.ReadValue(state.GetEntityByName("427door") + _baseEntityAngleOffset, out doorang);

                            // a check to prevent the game starting the game again right after you reset on the first map
                            // now this only happens if you're 10 units near the start point
                            if (state.PlayerPosition.Distance(startpos) <= 10f) 
                            {
                                bool wasplayerinstartpoint      = state.PrevPlayerPosition.BitEqualsXY(startpos);
                                bool isplayeroutsidestartpoint  = !state.PlayerPosition.BitEqualsXY(startpos);
                                bool isdoorangledifferent       = !doorang.BitEquals(doorstartang); // for reluctant ending
                                bool isviewangledifferent       = ang.Distance(startang) >= 0.1f;
                                bool isplayerteleported         = state.PrevPlayerPosition.Distance(spawnpos) >= 5f; 

                                if ((wasplayerinstartpoint && (isplayeroutsidestartpoint || isdoorangledifferent)) || isviewangledifferent && isplayerteleported)
                                {
                                    _resetFlag = true;
                                    //Debug.WriteLine("stanley start");
                                    return GameSupportResult.PlayerGainedControl;
                                }
                            }
                        }

                        if (state.PlayerPosition.DistanceXY(ending_insane_sector_origin) <= 1353) // insane ending
                        {

                            if (state.PlayerViewEntityIndex == ending_map1_cam_index &&
                                state.PrevPlayerViewEntityIndex == ending_insane_cam_index)
                            {
                                defaultend("insane");
                                EndOffsetTicks = 3;
                                return GameSupportResult.ManualSplit;
                            }

                        }
                        else if (state.PlayerViewEntityIndex == ending_map1_cam_index &&
                                state.PrevPlayerViewEntityIndex == 1 &&
                                state.PrevPlayerPosition.Distance(spawnpos) >= 5f)
                        {
                            defaultend("map1 blackout");
                            EndOffsetTicks = 4;
                            return GameSupportResult.ManualSplit;
                        }

                        if (state.PlayerPosition.DistanceXY(ending_broom_trig_origin) <= 50) // broom closet ending
                        {
                            return defaultend("broom");
                        }

                        int buttonpresses; // heaven ending
                        state.GameProcess.ReadValue(server.BaseAddress + buttonpasses, out buttonpresses);

                        if (buttonpresses >= 4)
                        {
                            var newbr = state.GetEntInfoByIndex(ending_heaven_br_index);

                            if (newbr.EntityPtr == IntPtr.Zero && (buttonpresses > ending_heaven_bpass1))
                            {
                                defaultend("heaven");
                                EndOffsetTicks = 1;
                                return GameSupportResult.ManualSplit;
                            }
                        }

                        // song, voice over ending
                        if (state.PlayerPosition.Distance(ending_outside_region) <= 232)
                        {
                            state.GameProcess.ReadValue(ending_song_brush + brushdisabledoffset, out ending_song_disabled);
                            state.GameProcess.ReadValue(ending_vo_button_index + buttonlockoffset, out ending_vo_button_locked);
                            var newtrig = state.GetEntInfoByIndex(ending_song_trig_index);

                            if ((newtrig.EntityPtr == IntPtr.Zero || ending_song_trig_index == -1) && ending_song_disabled == 0)
                            {
                                ending_song_oktoconsider = true;
                            }

                            if (ending_song_oktoconsider)
                            {
                                if (ending_song_disabled == 1)
                                {
                                    defaultend("song");
                                    ending_song_oktoconsider = false;
                                    EndOffsetTicks = -2000;
                                    return GameSupportResult.ManualSplit;
                                }
                            }

                            if (ending_vo_button_locked == 1 && ending_song_disabled == 0)
                            {
                                defaultend("voice over");
                                EndOffsetTicks = -14;
                                return GameSupportResult.ManualSplit;
                            }
                        }
                        
                        // whiteboard ending
                        if (state.PlayerPosition.Distance(ending_whiteboard_door_origin) <= 800 && 
                            state.PlayerPosition.X >= 1993 && state.PrevPlayerPosition.X <= 1993)
                        {
                            return defaultend("whiteboard");
                        }

                        // half of confusion ending
                        if (ending_confuse_flag)
                        {
                            ending_confuse_flag = false;
                            return defaultend("confusion");
                        }

                        //Debug.WriteLine(ending_vo_button_locked);
                        break;
                    }

                case "map2": //countdown, disco ending
                    {
                        if  (state.PlayerViewEntityIndex == ending_count_cam_index &&
                            state.PrevPlayerViewEntityIndex == 1)
                        {
                            defaultend("countdown");
                            EndOffsetTicks = -11;
                            return GameSupportResult.ManualSplit;
                        }

                        if (_resetFlag2 == false)
                        {
                            var text = state.GetEntInfoByIndex(ending_disco_text_index);
                            state.GameProcess.ReadValue(text.EntityPtr + _baseEntityAngleVelOffset, out ending_disco_angvel);
                            Debug.WriteLine(ending_disco_angvel);
                            if (ending_disco_angvel.Y == 20)
                            {
                                _resetFlag2 = true;
                                Debug.WriteLine("secret disco end");
                                return GameSupportResult.PlayerLostControl;
                            }
                        }
                        break;
                    }

                case "babygame": //art ending
                    {
                        if (state.PlayerViewEntityIndex == ending_art_cam_index &&
                            state.PrevPlayerViewEntityIndex == 1)
                        {
                            defaultend("art");
                            EndOffsetTicks = 3;
                            return GameSupportResult.ManualSplit;
                        }
                        break;
                    }

                case "freedom": //freedom ending
                    {

                        if (state.PlayerParentEntityHandle != -1
                            && state.PrevPlayerParentEntityHandle == -1)
                        {
                            defaultend("freedom");
                            EndOffsetTicks = 2;
                            return GameSupportResult.ManualSplit;
                        }
                        break;
                    }

                case "redstair": //escape ending
                    {
                        if (state.PlayerViewEntityIndex == ending_escape_cam_index
                            && state.PrevPlayerViewEntityIndex == 1)
                        {
                            defaultend("escape");
                            EndOffsetTicks = 3;
                            return GameSupportResult.ManualSplit;
                        }
                        break;
                    }

                case "incorrect": //choice ending
                    {
                        state.GameProcess.ReadValue(client.BaseAddress + drawcreditsoffset, out credits);
                        if (credits == 1)
                        {
                            return defaultend("choice");
                        }
                        break;
                    }

                case "testchmb_a_00": // games, stuck ending
                    {
                        if (state.PrevPlayerViewEntityIndex == 1 &&
                            state.PlayerViewEntityIndex == ending_games_cam_index)
                        {
                            defaultend("games");
                            EndOffsetTicks = 3;
                            return GameSupportResult.ManualSplit;
                        }

                        if (state.GetEntityPos(ending_stuck_box_index_2).Distance(new Vector3f(-844f, -1043f, 676f)) <= 50)
                        {
                            ending_stuck_box_index = state.GetEntIndexByNameMultiple("box", ending_stuck_box_index_2);
                        }

                        if (ending_stuck_box_index != -1)
                        {
                            var box = state.GetEntInfoByIndex(ending_stuck_box_index);
                            Vector3f rot;
                            state.GameProcess.ReadValue(box.EntityPtr + _baseEntityAngleOffset, out rot);

                            double dist;
                            double roty = rot.Y * (Math.PI / 180); // radians

                            dist = Math.Abs(40f * Math.Cos(roty)) + Math.Abs(40f * Math.Sin(roty)); // width of the box including its oritentation

                            // almost correct
                            if (state.GetEntityPos(ending_stuck_box_index).X - dist/2 <= -960)
                            {
                                return defaultend("stuck");
                            }
                            //Debug.WriteLine(dist + " " + state.PlayerPosition.X + " " + (state.GetEntityPos(ending_stuck_box_index).X - dist / 2));
                        }
                        break;
                    }

                case "map_death": //museum ending
                    {
                        if (commandptr != IntPtr.Zero)
                        {
                            string cmd;
                            state.GameProcess.ReadString(commandptr, ReadStringType.ASCII, 9, out cmd);

                            if (cmd.ToLower() == "stopsound")
                            {
                                return defaultend("museum");
                            }
                        }
                        break;
                    }
                case "seriousroom": //serious ending
                    {
                        if (ending_serious_count == 3)
                        {
                            return defaultend("serious");
                        }
                        break;
                    }
                case "zending": // space, zending ending
                    {
                        if (state.PrevPlayerPosition.X <= -7000 && 
                            state.PlayerPosition.X >= -400)
                        {
                            Debug.WriteLine("space ending");
                            return GameSupportResult.PlayerLostControl;
                        }

                        if (state.PlayerViewEntityIndex == ending_zending_cam_index &&
                            state.PrevPlayerViewEntityIndex == 1)
                        {
                            defaultend("zending");
                            EndOffsetTicks = 4;
                            return GameSupportResult.ManualSplit;
                        }
                        break;
                    }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
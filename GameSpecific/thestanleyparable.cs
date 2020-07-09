using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;


namespace LiveSplit.SourceSplit.GameSpecific
{
    class TheStanleyParable : GameSupport
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
        // heaven:      when the tsp_buttonpass counter is 4 or higher AND it is increased when the final button in _stanley's room is pressed
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
        private const int _angleOffset = 2812;
        private const int _brushDisabledOffset = 864;
        private const int _buttonLockOffset = 1040;
        private const int _drawCreditsOffset = 16105320;
        private const int _clientCommandInputOffset = 6726216;
        private const int _buttonPasses = 8263812;
        private int _credits;
        IntPtr _commandptr;


        // endings
        //Vector3f oldpos = new Vector3f(-154.778f, -205.209f, 0f); TODO: add 2nd start position which is where you end up after the intro cutscene
        Vector3f _startPos = new Vector3f(-200f, -208f, 0.03125f);
        Vector3f _startAng = new Vector3f(0f, 90f, 0f);
        Vector3f _spawnPos = new Vector3f(5152f, 776f, 3328f);
        Vector3f _doorStartAng = new Vector3f(0f, 360f, 0f);
        private IntPtr _ending_Song_Brush;
        private int _ending_Song_Disabled;
        private static bool _ending_Song_OkToConsider;
        private int _ending_Song_Trig_Index;
        private IntPtr _ending_VO_Button_Index;
        private int _ending_VO_Button_Locked;
        private Vector3f _ending_Outside_Region = new Vector3f(-704f, 256f, -256f);
        private int _ending_Heaven_Br_Index;
        private int _ending_Heaven_BPass1;
        private int _ending_Escape_Cam_Index;
        private int _Ending_Map1_Cam_Index;
        private int _ending_Count_Cam_Index;
        private int ending_Art_Cam_Index;
        private int _ending_Games_Cam_Index;
        Vector3f _ending_Broom_Trig_Origin = new Vector3f(-592f, 2208f, 64f);
        Vector3f _ending_Insane_Sector_Origin = new Vector3f(-6072f, 888f, 0);
        private int _ending_Insane_Cam_Index;
        private static int _ending_Serious_Count;
        private int _ending_Disco_Text_Index;
        Vector3f _ending_Disco_AngVel;
        private int _ending_Stuck_Box_Index;
        private int _ending_Stuck_Box_Index_2;
        private int _ending_Zending_Cam_Index;
        Vector3f _ending_Whiteboard_Door_Origin = new Vector3f(1988f, -1792f, -1992f);
        private static bool _ending_Confuse_Flag = false;

        public static bool _stanley = false;

        public TheStanleyParable()
        {
            _stanley = true;
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            //this.FirstMap = "map1"; <= we go back to this map a lot so its better to just disable this
            //this.LastMap = NAH;
            this.RequiredProperties = PlayerProperties.ViewEntity | PlayerProperties.Position | PlayerProperties.ParentEntity;
        }

        public override void OnTimerReset(bool resetflagto)
        {
            _resetFlag = resetflagto;
            _resetFlag2 = false;
            _ending_Serious_Count = 0;
            _ending_Song_OkToConsider = false;
            _ending_Confuse_Flag = false;
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

            _ending_Serious_Count = 0;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            switch (state.CurrentMap.ToLower())
            {
                case "map1":
                    {
                        this._Ending_Map1_Cam_Index = state.GetEntIndexByName("cam_black");
                        this._ending_Heaven_Br_Index = state.GetEntIndexByName("427compbr");
                        state.GameProcess.ReadValue(server.BaseAddress + _buttonPasses, out _ending_Heaven_BPass1);
                        this._ending_Insane_Cam_Index = state.GetEntIndexByName("mariella_camera");
                        this._ending_Song_Brush = state.GetEntityByName("errorgag");
                        this._ending_Song_Trig_Index = state.GetEntIndexByPos(-620f, 256f, -208f);
                        this._ending_VO_Button_Index = state.GetEntInfoByIndex(state.GetEntIndexByPos(-703f, 295f, -198f)).EntityPtr;

                        state.GameProcess.ReadValue(_ending_Song_Brush + _brushDisabledOffset, out _ending_Song_Disabled);
                        if ((state.GetEntInfoByIndex(_ending_Song_Trig_Index).EntityPtr == IntPtr.Zero || _ending_Song_Trig_Index == -1) && _ending_Song_Disabled == 0)
                        {
                            SourceSplitComponent.ResetSpecialSplit();
                        }
                        break;
                    }
                case "map2":
                    {
                        this._ending_Count_Cam_Index = state.GetEntIndexByName("cam_white");
                        this._ending_Disco_Text_Index = state.GetEntIndexByName("emotionboothDrot");
                        break;
                    }
                case "redstair":
                    {
                        this._ending_Escape_Cam_Index = state.GetEntIndexByName("blackcam");
                        break;
                    }
                case "babygame":
                    {
                        this.ending_Art_Cam_Index = state.GetEntIndexByName("whitecamera");
                        break;
                    }
                case "map_death":
                case "map":
                    {
                        _commandptr = engine.BaseAddress + _clientCommandInputOffset;
                        break;
                    }
                case "testchmb_a_00":
                    {
                        this._ending_Games_Cam_Index = state.GetEntIndexByName("blackoutend");
                        _ending_Stuck_Box_Index_2 = state.GetEntIndexByName("box");
                        break;
                    }
                case "seriousroom":
                    {
                        _ending_Serious_Count += 1;
                        break;
                    }
                case "zending":
                    {
                        _ending_Zending_Cam_Index = state.GetEntIndexByName("cam_dead");
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
                state.GameProcess.ReadString(_commandptr, ReadStringType.ASCII, 12, out cmd2);
                if (cmd2 == "tsp_reload 5")
                {
                    _ending_Confuse_Flag = true;
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

                        if (!_resetFlag)
                        {
                            Vector3f ang;
                            Vector3f doorAng;

                            state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _angleOffset, out ang);
                            state.GameProcess.ReadValue(state.GetEntityByName("427door") + _baseEntityAngleOffset, out doorAng);

                            // a check to prevent the game starting the game again right after you reset on the first map
                            // now this only happens if you're 10 units near the start point
                            if (state.PlayerPosition.Distance(_startPos) <= 10f) 
                            {
                                bool wasPlayerInStartPoint      = state.PrevPlayerPosition.BitEqualsXY(_startPos);
                                bool isPlayerOutsideStartPoint  = !state.PlayerPosition.BitEqualsXY(_startPos);
                                bool isDoorAngleDifferent       = !doorAng.BitEquals(_doorStartAng); // for reluctant ending
                                bool isViewAngleDifferent       = ang.Distance(_startAng) >= 0.1f;
                                bool isPlayerTeleported         = state.PrevPlayerPosition.Distance(_spawnPos) >= 5f; 

                                if ((wasPlayerInStartPoint && (isPlayerOutsideStartPoint || isDoorAngleDifferent)) || isViewAngleDifferent && isPlayerTeleported)
                                {
                                    _resetFlag = true;
                                    //Debug.WriteLine("_stanley start");
                                    return GameSupportResult.PlayerGainedControl;
                                }
                            }
                        }

                        if (state.PlayerPosition.DistanceXY(_ending_Insane_Sector_Origin) <= 1353) // insane ending
                        {

                            if (state.PlayerViewEntityIndex == _Ending_Map1_Cam_Index &&
                                state.PrevPlayerViewEntityIndex == _ending_Insane_Cam_Index)
                            {
                                defaultend("insane");
                                EndOffsetTicks = 3;
                                return GameSupportResult.ManualSplit;
                            }

                        }
                        else if (state.PlayerViewEntityIndex == _Ending_Map1_Cam_Index &&
                                state.PrevPlayerViewEntityIndex == 1 &&
                                state.PrevPlayerPosition.Distance(_spawnPos) >= 5f)
                        {
                            defaultend("map1 blackout");
                            EndOffsetTicks = 4;
                            return GameSupportResult.ManualSplit;
                        }

                        if (state.PlayerPosition.DistanceXY(_ending_Broom_Trig_Origin) <= 50) // broom closet ending
                        {
                            return defaultend("broom");
                        }

                        int buttonPresses; // heaven ending
                        state.GameProcess.ReadValue(server.BaseAddress + _buttonPasses, out buttonPresses);

                        if (buttonPresses >= 4)
                        {
                            var newbr = state.GetEntInfoByIndex(_ending_Heaven_Br_Index);

                            if (newbr.EntityPtr == IntPtr.Zero && (buttonPresses > _ending_Heaven_BPass1))
                            {
                                defaultend("heaven");
                                EndOffsetTicks = 1;
                                return GameSupportResult.ManualSplit;
                            }
                        }

                        // song, voice over ending
                        if (state.PlayerPosition.Distance(_ending_Outside_Region) <= 232)
                        {
                            state.GameProcess.ReadValue(_ending_Song_Brush + _brushDisabledOffset, out _ending_Song_Disabled);
                            state.GameProcess.ReadValue(_ending_VO_Button_Index + _buttonLockOffset, out _ending_VO_Button_Locked);
                            var newTrig = state.GetEntInfoByIndex(_ending_Song_Trig_Index);

                            if ((newTrig.EntityPtr == IntPtr.Zero || _ending_Song_Trig_Index == -1) && _ending_Song_Disabled == 0)
                            {
                                _ending_Song_OkToConsider = true;
                            }

                            if (_ending_Song_OkToConsider)
                            {
                                if (_ending_Song_Disabled == 1)
                                {
                                    defaultend("song");
                                    _ending_Song_OkToConsider = false;
                                    EndOffsetTicks = -2000;
                                    return GameSupportResult.ManualSplit;
                                }
                            }

                            if (_ending_VO_Button_Locked == 1 && _ending_Song_Disabled == 0)
                            {
                                defaultend("voice over");
                                EndOffsetTicks = -14;
                                return GameSupportResult.ManualSplit;
                            }
                        }
                        
                        // whiteboard ending
                        if (state.PlayerPosition.Distance(_ending_Whiteboard_Door_Origin) <= 800 && 
                            state.PlayerPosition.X >= 1993 && state.PrevPlayerPosition.X <= 1993)
                        {
                            return defaultend("whiteboard");
                        }

                        // half of confusion ending
                        if (_ending_Confuse_Flag)
                        {
                            _ending_Confuse_Flag = false;
                            return defaultend("confusion");
                        }

                        //Debug.WriteLine(_ending_VO_Button_Locked);
                        break;
                    }

                case "map2": //countdown, disco ending
                    {
                        if  (state.PlayerViewEntityIndex == _ending_Count_Cam_Index &&
                            state.PrevPlayerViewEntityIndex == 1)
                        {
                            defaultend("countdown");
                            EndOffsetTicks = -11;
                            return GameSupportResult.ManualSplit;
                        }

                        if (!_resetFlag2)
                        {
                            var text = state.GetEntInfoByIndex(_ending_Disco_Text_Index);
                            state.GameProcess.ReadValue(text.EntityPtr + _baseEntityAngleVelOffset, out _ending_Disco_AngVel);
                            Debug.WriteLine(_ending_Disco_AngVel);
                            if (_ending_Disco_AngVel.Y == 20)
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
                        if (state.PlayerViewEntityIndex == ending_Art_Cam_Index &&
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
                        if (state.PlayerViewEntityIndex == _ending_Escape_Cam_Index
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
                        state.GameProcess.ReadValue(client.BaseAddress + _drawCreditsOffset, out _credits);
                        if (_credits == 1)
                        {
                            return defaultend("choice");
                        }
                        break;
                    }

                case "testchmb_a_00": // games, stuck ending
                    {
                        if (state.PrevPlayerViewEntityIndex == 1 &&
                            state.PlayerViewEntityIndex == _ending_Games_Cam_Index)
                        {
                            defaultend("games");
                            EndOffsetTicks = 3;
                            return GameSupportResult.ManualSplit;
                        }

                        if (state.GetEntityPos(_ending_Stuck_Box_Index_2).Distance(new Vector3f(-844f, -1043f, 676f)) <= 50 || _ending_Stuck_Box_Index_2 == -1)
                        {
                            // the originally detected box is actually a template which is used to spawn
                            // the real box, so we have to recheck with an exception
                            _ending_Stuck_Box_Index = state.GetEntIndexByName("box", _ending_Stuck_Box_Index_2);
                        }

                        Debug.WriteLine(_ending_Stuck_Box_Index_2);

                        if (_ending_Stuck_Box_Index != -1)
                        {
                            var box = state.GetEntInfoByIndex(_ending_Stuck_Box_Index);
                            Vector3f rot;
                            state.GameProcess.ReadValue(box.EntityPtr + _baseEntityAngleOffset, out rot);

                            double dist;
                            double rotY = rot.Y * (Math.PI / 180); // radians

                            dist = Math.Abs(40f * Math.Cos(rotY)) + Math.Abs(40f * Math.Sin(rotY)); // width of the box including its oritentation

                            // almost correct
                            if (state.GetEntityPos(_ending_Stuck_Box_Index).X - dist/2 <= -960)
                            {
                                return defaultend("stuck");
                            }
                            //Debug.WriteLine(dist + " " + state.PlayerPosition.X + " " + (state.GetEntityPos(_ending_Stuck_Box_Index).X - dist / 2));
                        }
                        break;
                    }

                case "map_death": //museum ending
                    {
                        if (_commandptr != IntPtr.Zero)
                        {
                            string cmd;
                            state.GameProcess.ReadString(_commandptr, ReadStringType.ASCII, 9, out cmd);

                            if (cmd.ToLower() == "stopsound")
                            {
                                return defaultend("museum");
                            }
                        }
                        break;
                    }
                case "seriousroom": //serious ending
                    {
                        if (_ending_Serious_Count == 3)
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

                        if (state.PlayerViewEntityIndex == _ending_Zending_Cam_Index &&
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
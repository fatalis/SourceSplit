using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TheStanleyParable : GameSupport
    {
        // FOR THE RETAIL HD REMAKE
        // TODO: add original mod and demo version support

        // start: when player first moves OR their view angle first changes OR when they close the door while standing still AND they mustve been teleported

        // endings:
        // freedom:     when player's parent entity handle changes from nothing
        // countdown:   when the player's view entity changes to the final whiteout camera AND the screen's fade alpha reaches 255
        // art:         when the player's view entity changes to the final camera
        // reluctant:   when the player's view entity changes to the final blackout camera AND the player is within 10 units of the 427 room
        // escape:      when the player's view entity changes to the final camera
        // broom:       when tsp_broompass is entered   
        // choice:      when stanley_drawcredits is set to 1
        // confusion:   when the cmd point_clientcommand entity gets fed "tsp_reload 5", this only gets detected in onsessionend, the split happens on map1
        // games:       when the player's view entity changes to the final blackout camera
        // heaven:      when the tsp_buttonpass counter is 4 or higher AND it is increased when the final button in _stanley's room is pressed
        // insane:      when the player's view entity changes to the final blackout camera AND the player is wihtin the rooms before the cutscene
        // museum:      when the cmd point_clientcommand entity gets fed "StopSound"
        // serious:     when you've visited the seriousroom map 3 times
        // disco:       when the y axis speed of the entity that rotates the text is 20
        // stuck:       when the ending math_counter goes from 1 to 2
        // song:        when the timer on the target logic_choreographed_scene entity hits 33.333, which is when the song plays
        // voice over:  when the timer on the target logic_choreographed_scene entity hits 0.075, which is when the narrator exclaims "Ah"
        // space:       when the previous player's X pos is less than -7000 and current is higher than -400
        // zending:     when the player view entity switches from the player to the final camera
        // whiteboard:  when the previous player's X pos is =< 1993 and current is >= 1993

        private bool _onceFlag;
        public static bool _resetFlag;

        ProcessModuleWow64Safe client;
        ProcessModuleWow64Safe engine;
        ProcessModuleWow64Safe server;

        // offsets
        private int _baseEntityAngleOffset = -1;
        private int _baseEntityAngleVelOffset = -1;
        private const int _angleOffset = 2812;
        private const int _drawCreditsOffset = 16105320;
        private const int _clientCommandInputOffset = 6726216;
        private const int _buttonPasses = 8263812;
        private const int _logicChoreoTimerOffset = 0x3b4;
        private const int _mathCounterCurValueOffset = 0x368;


        // start
        //Vector3f oldpos = new Vector3f(-154.778f, -205.209f, 0f); TODO: add 2nd start position which is where you end up after the intro cutscene
        Vector3f _startPos = new Vector3f(-200f, -208f, 0.03125f);
        Vector3f _startAng = new Vector3f(0f, 90f, 0f);
        Vector3f _spawnPos = new Vector3f(5152f, 776f, 3328f);
        Vector3f _doorStartAng = new Vector3f(0f, 360f, 0f);

        // endings
        private MemoryWatcherList _endings_Watcher = new MemoryWatcherList();
        private StringWatcher _latest_Client_Cmd;

        private MemoryWatcher<float> _endingSongTimer;
        private MemoryWatcher<float> _endingVOTimer;
        private int _endingHeavenBrIndex;
        private int _endingHeavenBPass1;
        private int _endingEscapeCamIndex;
        private int _endingMap1CamIndex;
        private int _endingCountCamIndex;
        private int endingArtCamIndex;
        private int _endingGamesCamIndex;
        Vector3f _endingInsaneSectorOrigin = new Vector3f(-6072f, 888f, 0);
        private int _endingInsaneCamIndex;
        private static int _endingSeriousCount;
        private MemoryWatcher<Vector3f> _endingDiscoAngVel;
        private MemoryWatcher<float> _endingStuckEndingCount;
        private MemoryWatcher<int> _credits;
        private MemoryWatcher<int> _endingCountFadeAlpha;
        private int _endingZendingCamIndex;
        Vector3f _endingWhiteboardDoorOrigin = new Vector3f(1988f, -1792f, -1992f);
        private static bool _endingConfuseFlag = false;

        public TheStanleyParable()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            //this.FirstMap = "map1"; <= we go back to this map a lot so its better to just disable this
            //this.LastMap = NAH;
            this.RequiredProperties = PlayerProperties.ViewEntity | PlayerProperties.Position | PlayerProperties.ParentEntity;
        }

        // recreation of the findnextentity function to get access to server-side entities that aren't networked over to the client
        // which can't be found using the old functions
        IntPtr GetNextEntPtr(GameState state, IntPtr prev)
        {
            if (prev == IntPtr.Zero)
            {
                return state.GameProcess.ReadPointer(state.GameOffsets.GlobalEntityListPtr);
            }
            else
            {
                // these variable names are guesses!
                uint index;
                uint entInfo = (uint)state.GameProcess.ReadPointer(prev + 724);
                if (entInfo == 0xffffffff)
                    index = 0x1fff;
                else
                    index = entInfo & 0xffff;

                IntPtr ptr1 = (IntPtr)((uint)state.GameOffsets.GlobalEntityListPtr + index * 24);
                IntPtr ptr2 = state.GameProcess.ReadPointer(ptr1 + 0xC);

                if (ptr1 != IntPtr.Zero && ptr2 != IntPtr.Zero)
                    return state.GameProcess.ReadPointer(ptr2);
            }
            return IntPtr.Zero;
        }

        IntPtr FindEntByName(GameState state, string name)
        {
            IntPtr entPtr = GetNextEntPtr(state, IntPtr.Zero);
            while (entPtr != IntPtr.Zero)
            {
                string name2 = state.GameProcess.ReadString(state.GameProcess.ReadPointer(entPtr + state.GameOffsets.BaseEntityTargetNameOffset), 128);
                if (name2 != null && name2.ToLower() == name.ToLower())
                {
                    return entPtr;
                }
                entPtr = GetNextEntPtr(state, entPtr);
            }
            return IntPtr.Zero;
        }

        // Shorthands
        public GameSupportResult DefaultEnd(string endingname)
        {
            _onceFlag = true;
            Debug.WriteLine(endingname + " ending");
            return GameSupportResult.PlayerLostControl;
        }

        public bool EvaluateLatestClientCmd(string cmd, int length)
        {
            if (_latest_Client_Cmd.Old == null || _latest_Client_Cmd.Current.Length < length)
            {
                return false;
            }
            return _latest_Client_Cmd.Current.Substring(0, length).ToLower() == cmd.ToLower() && _latest_Client_Cmd.Old.Substring(0, length).ToLower() != cmd.ToLower();
        }

        public bool EvaluateChangedViewIndex(GameState state, int prev, int now)
        {
            return state.PrevPlayerViewEntityIndex == prev && state.PlayerViewEntityIndex == now;
        }

        public override void OnTimerReset(bool resetflagto)
        {
            _resetFlag = resetflagto;
            _endingSeriousCount = 0;
            _endingConfuseFlag = false;
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

            _endings_Watcher.ResetAll();

            _endingSeriousCount = 0;
            _latest_Client_Cmd = new StringWatcher(engine.BaseAddress + _clientCommandInputOffset, 50);
            _endings_Watcher.Add(_latest_Client_Cmd);
            _credits = new MemoryWatcher<int>(client.BaseAddress + _drawCreditsOffset);
            _endings_Watcher.Add(_credits);

        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            switch (state.CurrentMap.ToLower())
            {
                case "map1":
                    {
                        this._endingMap1CamIndex = state.GetEntIndexByName("cam_black");
                        this._endingHeavenBrIndex = state.GetEntIndexByName("427compbr");
                        state.GameProcess.ReadValue(server.BaseAddress + _buttonPasses, out _endingHeavenBPass1);
                        this._endingInsaneCamIndex = state.GetEntIndexByName("mariella_camera");
                        this._endingSongTimer = new MemoryWatcher<float>(state.GetEntityByName("narratorerroryes") + _logicChoreoTimerOffset);
                        this._endingVOTimer = new MemoryWatcher<float>(state.GetEntityByName("narratorerrorno") + _logicChoreoTimerOffset);
                        _endings_Watcher.Add(_endingSongTimer);
                        _endings_Watcher.Add(_endingVOTimer);
                        break;
                    }
                case "map2":
                    {
                        this._endingCountCamIndex = state.GetEntIndexByName("cam_white");
                        this._endingDiscoAngVel = new MemoryWatcher<Vector3f>(state.GetEntityByName("emotionboothDrot") + _baseEntityAngleVelOffset);
                        this._endingCountFadeAlpha = new MemoryWatcher<int>(client.BaseAddress + 0xf63000);
                        _endings_Watcher.Add(_endingDiscoAngVel);
                        _endings_Watcher.Add(_endingCountFadeAlpha);
                        break;
                    }
                case "redstair":
                    {
                        this._endingEscapeCamIndex = state.GetEntIndexByName("blackcam");
                        break;
                    }
                case "babygame":
                    {
                        this.endingArtCamIndex = state.GetEntIndexByName("whitecamera");
                        break;
                    }
                case "testchmb_a_00":
                    {
                        _endingGamesCamIndex = state.GetEntIndexByName("blackoutend");
                        _endingStuckEndingCount = new MemoryWatcher<float>(FindEntByName(state, "buttonboxendingcount") + _mathCounterCurValueOffset);
                        _endings_Watcher.Add(_endingStuckEndingCount);
                        break;
                    }
                case "seriousroom":
                    {
                        _endingSeriousCount += 1;
                        break;
                    }
                case "zending":
                    {
                        _endingZendingCamIndex = state.GetEntIndexByName("cam_dead");
                        break;
                    }
            }

            _onceFlag = false;
        }

        public override void OnSessionEnd(GameState state)
        {
            if (state.CurrentMap.ToLower() == "map" && EvaluateLatestClientCmd("tsp_reload 5", 12)) // confusion ending
            {
                _endingConfuseFlag = true;
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            _endings_Watcher.UpdateAll(state.GameProcess);

            Debug.WriteLine(state.PlayerEntInfo.EntityPtr.ToString("X"));

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
                                    Debug.WriteLine("stanley start");
                                    return GameSupportResult.PlayerGainedControl;
                                }
                            }
                        }

                        if (state.PlayerPosition.DistanceXY(_endingInsaneSectorOrigin) <= 1353) // insane ending
                        {
                            if (EvaluateChangedViewIndex(state, _endingInsaneCamIndex, _endingMap1CamIndex))
                            {
                                DefaultEnd("insane");
                                EndOffsetTicks = 3;
                                return GameSupportResult.ManualSplit;
                            }

                        }
                        else if (EvaluateChangedViewIndex(state, 1, _endingMap1CamIndex) &&
                            state.PrevPlayerPosition.Distance(_spawnPos) >= 5f)
                        {
                            DefaultEnd("map1 blackout");
                            EndOffsetTicks = 4;
                            return GameSupportResult.ManualSplit;
                        }

                        if (EvaluateLatestClientCmd("tsp_broompass", 13)) // broom closet ending
                        {
                            return DefaultEnd("broom");
                        }

                        int buttonPresses; // heaven ending
                        state.GameProcess.ReadValue(server.BaseAddress + _buttonPasses, out buttonPresses);

                        if (buttonPresses >= 4)
                        {
                            var newbr = state.GetEntInfoByIndex(_endingHeavenBrIndex);

                            if (newbr.EntityPtr == IntPtr.Zero && (buttonPresses > _endingHeavenBPass1))
                            {
                                DefaultEnd("heaven");
                                EndOffsetTicks = 1;
                                return GameSupportResult.ManualSplit;
                            }
                        }

                        // song ending
                        if (_endingSongTimer.Current >= 33.333 && _endingSongTimer.Old < 33.333)
                        {
                            return DefaultEnd("song");
                        }

                        //voice over ending
                        if (_endingVOTimer.Current >= 0.075 && _endingVOTimer.Old < 0.075)
                        {
                            return DefaultEnd("voice over");
                        }

                        // whiteboard ending
                        if (state.PlayerPosition.Distance(_endingWhiteboardDoorOrigin) <= 800 &&
                            state.PlayerPosition.X >= 1993 && state.PrevPlayerPosition.X < 1993)
                        {
                            return DefaultEnd("whiteboard");
                        }

                        // half of confusion ending
                        if (_endingConfuseFlag)
                        {
                            _endingConfuseFlag = false;
                            return DefaultEnd("confusion");
                        }

                        break;
                    }

                case "map2": //countdown, disco ending
                    {
                        if (state.PlayerViewEntityIndex == _endingCountCamIndex
                            && _endingCountFadeAlpha.Old <= 255 && _endingCountFadeAlpha.Current == 255)
                        {
                            return DefaultEnd("countdown");
                        }

                        if (_endingDiscoAngVel.Current.Y == 20 && _endingDiscoAngVel.Old.Y != 20)
                        {
                            return DefaultEnd("secret disco");
                        }
                        break;
                    }

                case "babygame": //art ending
                    {
                        if (EvaluateChangedViewIndex(state, 1, endingArtCamIndex))
                        {
                            DefaultEnd("art");
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
                            DefaultEnd("freedom");
                            EndOffsetTicks = 2;
                            return GameSupportResult.ManualSplit;
                        }
                        break;
                    }

                case "redstair": //escape ending
                    {
                        if (EvaluateChangedViewIndex(state, 1, _endingEscapeCamIndex))
                        {
                            DefaultEnd("escape");
                            EndOffsetTicks = 3;
                            return GameSupportResult.ManualSplit;
                        }
                        break;
                    }

                case "incorrect": //choice ending
                    {
                        if (_credits.Current == 1 && _credits.Old == 0)
                        {
                            return DefaultEnd("choice");
                        }
                        break;
                    }

                case "testchmb_a_00": // games, stuck ending
                    {
                        if (EvaluateChangedViewIndex(state, 1, _endingGamesCamIndex))
                        {
                            DefaultEnd("games");
                            EndOffsetTicks = 3;
                            return GameSupportResult.ManualSplit;
                        }

                        if (_endingStuckEndingCount.Old == 1 && _endingStuckEndingCount.Current == 2)
                        {
                            return DefaultEnd("stuck");
                        }
                        break;
                    }

                case "map_death": //museum ending
                    {
                        if (EvaluateLatestClientCmd("stopsound", 9))
                        {
                            return DefaultEnd("museum");
                        }
                        break;
                    }
                case "seriousroom": //serious ending
                    {
                        if (_endingSeriousCount == 3)
                        {
                            return DefaultEnd("serious");
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

                        if (EvaluateChangedViewIndex(state, 1, _endingZendingCamIndex))
                        {
                            DefaultEnd("zending");
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

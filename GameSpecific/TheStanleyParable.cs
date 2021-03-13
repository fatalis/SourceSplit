using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TheStanleyParable : GameSupport
    {
        // DEMO:
        // start:           when the player's view entity index is 1 AND the player's position changed OR their view angle changed OR their view angle is different when they gain control
        // end:             when the player's view entity changes to final camera

        // MOD
        // start:           when the player just left the start position OR the player moved their view AND their view index is 1
        // endings:
            // freedom:     when the player's view entity changes to final camera
            // countdown:   3 ticks before when "stopsound" is sent to client command buffer
            // museum:      when the pod reaches within 0.05 units of the final path_track
            // insane:      when the player's view entity changes from insane camera's to final blackout camera's
            // games:       when the final out is fired to the game text entity to show "the end"
            // apartment:   when the player's view entity changes to final blackout camera AND the player is inside the room

        // HD REMAKE:
        // start:           when player first moves OR their view angle first changes OR when they close the door while standing still AND they mustve been teleported
        // endings:
            // freedom:     when player's parent entity handle changes from nothing
            // countdown:   when the player's view entity changes to the final whiteout camera AND the screen's fade alpha reaches 255
            // art:         when the player's view entity changes to the final camera
            // reluctant:   when the player's view entity changes to the final blackout camera AND the player is within 10 units of the 427 room
            // escape:      when the player's view entity changes to the final camera
            // broom:       when tsp_broompass is entered   
            // choice:      when the final output gets fired
            // confusion:   when the output to end the map is fired
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
        public static bool _resetFlagDemo;
        public static bool _resetFlagMod;
        private float _defFadeSplitTime;
        private float _defOutputSplitTime;

        private bool _isMod = false;
        //private const int _modClientModuleSize = 0x4cb000;
        private const float _angleEpsilon = 0.0005f;

        ProcessModuleWow64Safe client;
        ProcessModuleWow64Safe engine;
        ProcessModuleWow64Safe server;

        // offsets
        private int _baseEntityAngleOffset = -1;
        private int _baseEntityAngleVelOffset = -1;
        private const int _angleOffset = 2812;
        private const int _modAngleOffset = 0x40fe98;
        //private const int _drawCreditsOffset = 16105320;
        private const int _buttonPasses = 8263812;
        private const int _logicChoreoTimerOffset = 0x3b4;
        private const int _mathCounterCurValueOffset = 0x368;

        // start
        private MemoryWatcher<Vector3f> _playerViewAng;
        private MemoryWatcher<Vector3f> _startDoorAng;
        //Vector3f oldpos = new Vector3f(-154.778f, -205.209f, 0f); TODO: add 2nd start position which is where you end up after the intro cutscene
        private Vector3f _startPos = new Vector3f(-200f, -208f, 0.03125f);
        private Vector3f _startAng = new Vector3f(0f, 90f, 0f);
        private Vector3f _spawnPos = new Vector3f(5152f, 776f, 3328f);
        private Vector3f _doorStartAng = new Vector3f(0f, 360f, 0f);

        // demo start
        private Vector3f _demoStartPos = new Vector3f(-1284f, 404f, -107f);
        private Vector3f _demoStartAng = new Vector3f(0f, 180f, 0f);

        // mod start
        private Vector3f _modStartPos = new Vector3f(-334f, 182f, 44f);
        private Vector3f _modStartAng = new Vector3f(0f, -71.57f, 0f);

        // endings
        private MemoryWatcherList _endingsWatcher = new MemoryWatcherList();
        private StringWatcher _latestClientCmd;

        private MemoryWatcher<float> _endingSongTimer;
        private MemoryWatcher<float> _endingVOTimer;
        private int _endingHeavenBrIndex;
        private int _endingHeavenBPass1;
        private int _endingEscapeCamIndex;
        private int _endingMap1CamIndex;
        private int _endingCountCamIndex;
        private int endingArtCamIndex;
        private int _endingGamesCamIndex;
        private Vector3f _endingInsaneSectorOrigin = new Vector3f(-6072f, 888f, 0);
        private int _endingInsaneCamIndex;
        private static int _endingSeriousCount;
        private MemoryWatcher<Vector3f> _endingDiscoAngVel;
        private MemoryWatcher<float> _endingStuckEndingCount;
        private int _endingZendingCamIndex;
        private Vector3f _endingWhiteboardDoorOrigin = new Vector3f(1988f, -1792f, -1992f);

        // demo ending
        private int _demoEndingCamIndex;

        // mod endings
        private int _modEndingFreedomCamIndex;
        private int _modEndingGenericCamIndex;
        private int _modEndingInsaneCamIndex;
        private Vector3f _modEndingMuseumPodEndPos = new Vector3f(196f, 2812f, 994.772f);
        private MemoryWatcher<Vector3f> _modEndingMuseumPodPos;
        private Vector3f _modEndingApartmentSectorOrigin = new Vector3f(-3524f, 1368f, -620f);

        public TheStanleyParable()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.RequiredProperties = PlayerProperties.ViewEntity | PlayerProperties.Position | PlayerProperties.ParentEntity;
        }

        // Shorthands
        private GameSupportResult DefaultEnd(string endingname, int endoffset = 0)
        {
            this.EndOffsetTicks = endoffset;
            _onceFlag = true;
            Debug.WriteLine(endingname + " ending");

            if (endoffset != 0)
                return GameSupportResult.ManualSplit;
            else
                return GameSupportResult.PlayerLostControl;
        }

        private bool EvaluateLatestClientCmd(string cmd)
        {
            int length = cmd.Length;
            if (_latestClientCmd.Old == null || _latestClientCmd.Current.Length < length)
            {
                return false;
            }
            return _latestClientCmd.Current.Substring(0, length).ToLower() == cmd.ToLower() && _latestClientCmd.Old.Substring(0, length).ToLower() != cmd.ToLower();
        }

        private bool EvaluateChangedViewIndex(GameState state, int prev, int now)
        {
            return state.PrevPlayerViewEntityIndex == prev && state.PlayerViewEntityIndex == now;
        }

        private Vector3f Vector3fAbs(Vector3f A)
        {
            Vector3f B = new Vector3f();
            B.X = Math.Abs(A.X);
            B.Y = Math.Abs(A.Y);
            B.Z = Math.Abs(A.Z);
            return B;
        }

        private bool EvaluateChangedViewAngle(Vector3f prev, Vector3f now, Vector3f target)
        {
            return prev.Distance(target) <= _angleEpsilon && now.Distance(target) > _angleEpsilon;
        }

        private GameSupportResult DefaultFadeEnd(GameState state, float fadeSpeed, string ending)
        {
            float splitTime = state.FindFadeEndTime(fadeSpeed);
            _defFadeSplitTime = splitTime == 0f ? _defFadeSplitTime : splitTime;
            if (state.CompareToInternalTimer(_defFadeSplitTime, 0f, true))
            {
                _defFadeSplitTime = 0f;
                return DefaultEnd(ending);
            }
            else
                return GameSupportResult.DoNothing;
        }

        private GameSupportResult DefualtOutputEnd(GameState state, float splitTime, string ending)
        {
            _defOutputSplitTime = splitTime == 0f ? _defOutputSplitTime : splitTime;
            if (state.CompareToInternalTimer(_defOutputSplitTime, 0f))
            {
                _defOutputSplitTime = 0f;
                return DefaultEnd(ending);
            }
            else
                return GameSupportResult.DoNothing;
        }

        public override void OnTimerReset(bool resetflagto)
        {
            _resetFlag = _resetFlagDemo = _resetFlagMod = resetflagto;
            _endingSeriousCount = 0;
            _startAng.X = _demoStartAng.X = 0f;
        }

        public override void OnGameAttached(GameState state)
        {
            server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            client = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "client.dll");
            engine = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "engine.dll");

            Trace.Assert(server != null && client != null && engine != null);

            var serverScanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_angAbsRotation", state.GameProcess, serverScanner, out _baseEntityAngleOffset))
                Debug.WriteLine("CBaseEntity::m_angAbsRotation offset = 0x" + _baseEntityAngleOffset.ToString("X"));

            if (GameMemory.GetBaseEntityMemberOffset("m_vecAngVelocity", state.GameProcess, serverScanner, out _baseEntityAngleVelOffset))
                Debug.WriteLine("CBaseEntity::m_vecAngVelocity offset = 0x" + _baseEntityAngleVelOffset.ToString("X"));

            SigScanTarget _latest_Client_Trg = new SigScanTarget(0, Encoding.ASCII.GetBytes("ClientCommand, 0 length string supplied."));
            _latest_Client_Trg.OnFound = (proc, scanner, ptr) =>
            {
                byte[] b = BitConverter.GetBytes(ptr.ToInt32());
                var target = new SigScanTarget(2,
                    $"80 3D ?? ?? ?? ?? 00 75 ?? 68 {b[0]:X02} {b[1]:X02} {b[2]:X02} {b[3]:X02}"); // cmp byte ptr [clientcmdptr],00 
                IntPtr ptrPtr = scanner.Scan(target);
                if (ptrPtr == IntPtr.Zero)
                    return IntPtr.Zero;
                IntPtr ret;
                proc.ReadPointer(ptrPtr, out ret);
                Debug.WriteLine("CVEngineServer::ClientCommand szOut ptr is 0x" + ret.ToString("X"));
                return ret;
            };

            var engineScanner = new SignatureScanner(state.GameProcess, engine.BaseAddress, engine.ModuleMemorySize);

            _endingsWatcher.ResetAll();

            _endingSeriousCount = 0;
            _latestClientCmd = new StringWatcher(engineScanner.Scan(_latest_Client_Trg), 50);
            _endingsWatcher.Add(_latestClientCmd);
        }

        public override void OnGenericUpdate(GameState state)
        {
            if (state.CurrentMap.ToString() == "map" && state.HostState == HostState.GameShutdown)
                this.OnUpdate(state);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            _defFadeSplitTime = 0f;
            _defOutputSplitTime = 0f;

            _endingsWatcher.Remove(_playerViewAng);
            if (!_isMod)
                _playerViewAng = new MemoryWatcher<Vector3f>(state.PlayerEntInfo.EntityPtr + _angleOffset);
            else
                _playerViewAng = new MemoryWatcher<Vector3f>(client.BaseAddress + _modAngleOffset);

            _endingsWatcher.Add(_playerViewAng);


            switch (state.CurrentMap.ToLower())
            {
                case "stanley":
                    {
                        this._modEndingFreedomCamIndex = state.GetEntIndexByName("freedom_camera");
                        this._modEndingMuseumPodPos = new MemoryWatcher<Vector3f>(state.GetEntityByName("alt_pris_train") + state.GameOffsets.BaseEntityAbsOriginOffset);
                        this._modEndingGenericCamIndex = state.GetEntIndexByName("camera_credits");
                        this._modEndingInsaneCamIndex = state.GetEntIndexByName("mariella_camera");
                        _endingsWatcher.Add(_modEndingMuseumPodPos);
                        break;
                    }
                case "thedemo":
                    {
                        this._demoEndingCamIndex = state.GetEntIndexByName("democredits1");
                        break;
                    }
                case "map1":
                    {
                        this._startDoorAng = new MemoryWatcher<Vector3f>(state.GetEntityByName("427door") + _baseEntityAngleOffset);
                        _endingsWatcher.Add(_startDoorAng);
                        this._endingMap1CamIndex = state.GetEntIndexByName("cam_black");
                        this._endingHeavenBrIndex = state.GetEntIndexByName("427compbr");
                        state.GameProcess.ReadValue(server.BaseAddress + _buttonPasses, out _endingHeavenBPass1);
                        this._endingInsaneCamIndex = state.GetEntIndexByName("mariella_camera");
                        this._endingSongTimer = new MemoryWatcher<float>(state.GetEntityByName("narratorerroryes") + _logicChoreoTimerOffset);
                        this._endingVOTimer = new MemoryWatcher<float>(state.GetEntityByName("narratorerrorno") + _logicChoreoTimerOffset);
                        _endingsWatcher.Add(_endingSongTimer);
                        _endingsWatcher.Add(_endingVOTimer);
                        break;
                    }
                case "map2":
                    {
                        this._endingCountCamIndex = state.GetEntIndexByName("cam_white");
                        this._endingDiscoAngVel = new MemoryWatcher<Vector3f>(state.GetEntityByName("emotionboothDrot") + _baseEntityAngleVelOffset);
                        _endingsWatcher.Add(_endingDiscoAngVel);
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
                        _endingStuckEndingCount = new MemoryWatcher<float>(state.GetEntityByName("buttonboxendingcount") + _mathCounterCurValueOffset);
                        _endingsWatcher.Add(_endingStuckEndingCount);
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

        public override GameSupportResult OnUpdate(GameState state)
        {
            _endingsWatcher.UpdateAll(state.GameProcess);

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            switch (state.CurrentMap.ToLower())
            {
                default:
                    {
                        return GameSupportResult.DoNothing;
                    }

                // demo freedom, countdown, insane, museum, apartment
                case "stanley":
                    {
                        // start
                        if (!_resetFlagMod)
                        {
                            bool hasPlayerJustLeftStartPoint    = !state.PlayerPosition.BitEqualsXY(_modStartPos) && state.PrevPlayerPosition.BitEqualsXY(_modStartPos);
                            bool hasPlayerMovedView             = state.PlayerPosition.BitEqualsXY(_modStartPos)
                                                                && EvaluateChangedViewAngle(_playerViewAng.Old, _playerViewAng.Current, _modStartAng);
                            bool isViewEntityCorrect            = state.PlayerViewEntityIndex == 1;

                            if ((hasPlayerJustLeftStartPoint || hasPlayerMovedView) && isViewEntityCorrect)
                            {
                                _resetFlagDemo = true;
                                Debug.WriteLine("mod start");
                                return GameSupportResult.PlayerGainedControl;
                            }
                        }

                        // freedom ending
                        if (EvaluateChangedViewIndex(state, 1, _modEndingFreedomCamIndex))
                        {
                            return DefaultEnd("mod freedom");
                        }

                        // countdown ending
                        if (EvaluateLatestClientCmd("stopsound"))
                        {
                            return DefaultEnd("mod countdown", 4);
                        }

                        // museum ending
                        if (_modEndingMuseumPodPos.Current.DistanceXY(_modEndingMuseumPodEndPos) <= 0.05f &&
                            _modEndingMuseumPodPos.Old.DistanceXY(_modEndingMuseumPodEndPos) > 0.05f)
                        {
                            return DefaultEnd("mod museum");
                        }

                        // insane ending
                        if (EvaluateChangedViewIndex(state, _modEndingInsaneCamIndex, _modEndingGenericCamIndex))
                        {
                            return DefaultEnd("mod insane");
                        }

                        // apartment ending
                        if (EvaluateChangedViewIndex(state, 1, _modEndingGenericCamIndex) &&
                            state.PlayerPosition.DistanceXY(_modEndingApartmentSectorOrigin) <= 281f &&
                            state.PlayerPosition.Z <= -544f)
                        {
                            return DefaultEnd("mod apartment");
                        }

                        break;
                    }
                // demo games
                case "trainstation":
                    {
                        float splitTime = state.FindOutputFireTime("the_end", 2);
                        return DefualtOutputEnd(state, splitTime, "mod games");
                    }

                // tsp demo start and end
                case "thedemo":
                    {
                        if (!_resetFlagDemo)
                        {
                            if (Math.Abs(_playerViewAng.Old.X - _playerViewAng.Current.X) < _angleEpsilon && _playerViewAng.Current.Y == _demoStartAng.Y)
                                _demoStartAng.X = _playerViewAng.Current.X;

                            bool hasPlayerJustLeftStartPoint    = state.PrevPlayerPosition.BitEqualsXY(_demoStartPos) && !state.PlayerPosition.BitEqualsXY(_demoStartPos);
                            bool isPlayerViewEntityCorrect      = state.PlayerViewEntityIndex == 1;
                            bool hasPlayerMovedView             = EvaluateChangedViewAngle(Vector3fAbs(_playerViewAng.Old), Vector3fAbs(_playerViewAng.Current), _demoStartAng);
                            bool isViewAngleChangedEarly        = state.PrevPlayerPosition.BitEqualsXY(_demoStartPos)
                                                                && EvaluateChangedViewAngle(Vector3fAbs(_playerViewAng.Old), Vector3fAbs(_playerViewAng.Current), _demoStartAng);

                            if ((hasPlayerJustLeftStartPoint || hasPlayerMovedView || isViewAngleChangedEarly) && isPlayerViewEntityCorrect)
                            {
                                _resetFlagDemo = true;
                                Debug.WriteLine("stanley demo start");
                                return GameSupportResult.PlayerGainedControl;
                            }
                        }

                        if (EvaluateChangedViewIndex(state, 1, _demoEndingCamIndex))
                        {
                            return DefaultEnd("stanley demo");
                        }
                        break;
                    }

                case "map1": // beginning, death, reluctant, whiteboard, broom closet, song, voice over, cold feet, insane, apartment, heaven ending
                    {
                        if (!_resetFlag)
                        {
                            // your view angle slowly drifts upward for some reason so we'll have to readjust
                            if (Math.Abs(_playerViewAng.Old.X - _playerViewAng.Current.X) < _angleEpsilon && _playerViewAng.Current.Y == _startAng.Y)
                                _startAng.X = _playerViewAng.Current.X;

                            // a check to prevent the game starting the game again right after you reset on the first map
                            // now this only happens if you're 10 units near the start point
                            if (state.PlayerPosition.Distance(_startPos) <= 10f)
                            {
                                bool wasPlayerInStartPoint      = state.PrevPlayerPosition.BitEqualsXY(_startPos);
                                bool isPlayerOutsideStartPoint  = !state.PlayerPosition.BitEqualsXY(_startPos);
                                bool hasDoorAngleChanged        = _startDoorAng.Old.BitEquals(_doorStartAng) && !_startDoorAng.Current.BitEquals(_doorStartAng); // for reluctant ending
                                bool hasPlayerViewAngleChanged  = !isPlayerOutsideStartPoint && EvaluateChangedViewAngle(_playerViewAng.Old, _playerViewAng.Current, _startAng);
                                bool isPlayerTeleported         = state.PrevPlayerPosition.Distance(_spawnPos) >= 5f;

                                if ((wasPlayerInStartPoint && (isPlayerOutsideStartPoint || hasDoorAngleChanged)) || hasPlayerViewAngleChanged && isPlayerTeleported)
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
                                return DefaultEnd("insane", 3);
                            }

                        }
                        else if (EvaluateChangedViewIndex(state, 1, _endingMap1CamIndex) &&
                            state.PrevPlayerPosition.Distance(_spawnPos) >= 5f)
                        {
                            return DefaultEnd("map1 blackout", 4);
                        }

                        if (EvaluateLatestClientCmd("tsp_broompass")) // broom closet ending
                        {
                            return DefaultEnd("broom");
                        }

                        // heaven ending
                        state.GameProcess.ReadValue(server.BaseAddress + _buttonPasses, out int buttonPresses);

                        if (buttonPresses >= 4)
                        {
                            var newbr = state.GetEntInfoByIndex(_endingHeavenBrIndex);

                            if (newbr.EntityPtr == IntPtr.Zero && (buttonPresses > _endingHeavenBPass1))
                            {
                                return DefaultEnd("heaven", 1);
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

                        break;
                    }

                case "map2": //countdown, disco ending
                    {
                        if (state.PlayerViewEntityIndex == _endingCountCamIndex)
                        {
                            // some really weird floating point inprecision happening here for some reason...
                            return DefaultFadeEnd(state, -5222.399902f, "countdown");
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
                            return DefaultEnd("art", 3);
                        }
                        break;
                    }

                case "freedom": //freedom ending
                    {
                        if (state.PlayerParentEntityHandle != -1
                            && state.PrevPlayerParentEntityHandle == -1)
                        {
                            return DefaultEnd("freedom", 2);
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
                        float splitTime = state.FindOutputFireTime("smallnewtimerelay", 2);
                        return DefualtOutputEnd(state, splitTime, "choice");
                    }

                case "testchmb_a_00": // games, stuck ending
                    {
                        if (EvaluateChangedViewIndex(state, 1, _endingGamesCamIndex))
                        {
                            return DefaultEnd("games", 3);
                        }

                        if (_endingStuckEndingCount.Old == 1 && _endingStuckEndingCount.Current == 2)
                        {
                            return DefaultEnd("stuck");
                        }
                        break;
                    }

                case "map_death": //museum ending
                    {
                        float splitTime = state.FindOutputFireTime("cmd", "command", "stopsound", 2);
                        return DefualtOutputEnd(state, splitTime, "museum");;
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
                            return DefaultEnd("zending", 4);
                        }
                        break;
                    }
                case "map":
                    {
                        float splitTime = state.FindOutputFireTime("cmd", 2);
                        _defOutputSplitTime = splitTime == 0f ? _defOutputSplitTime : splitTime;
                        if (state.CompareToInternalTimer(_defOutputSplitTime, 0f, false, true))
                        {
                            DefaultEnd("confusion");
                            QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                        }
                        break;
                    }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

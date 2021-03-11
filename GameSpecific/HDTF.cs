using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HDTF : GameSupport
    {
        // start:   if IL had not been finished and 
        //          AND if start video isnt deleted and the video finishes playing 
        //          XOR if the start video is deleted and the map is newly spawned
        // ending:  when the blocker brush entity is killed

        private bool _onceFlag;
        private static bool _resetFlag;
        private static bool _tutResetFlag = true;
        private int _basePlayerLaggedMovementOffset = -1;

        private int _blockerIndex;
        private int _baseEntityHealthOffset = -1;
        private Vector3f _startPos = new Vector3f(772f, -813f, 164f);

        private MemoryWatcher<int> _playerHP;
        private MemoryWatcher<byte> _isInCutscene;
        private MemoryWatcher<float> _playerLaggedMovementValue;
        private MemoryWatcherList _watcher = new MemoryWatcherList();

        public HDTF()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap2 = "a0c0p0"; // boot camp
            this.LastMap = "a4c1p2";
            this.RequiredProperties = PlayerProperties.Position;
        }
        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            ProcessModuleWow64Safe bink = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "video_bink.dll");
          
            Trace.Assert(server != null && bink != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _basePlayerLaggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _basePlayerLaggedMovementOffset.ToString("X"));


            _watcher.ResetAll();

            // i would've sigscanned this but this dll is a 3rd party thing anyways so its unlikely to change between versions
            // and the game crashes when i try to debug it so oh well...
            _isInCutscene = new MemoryWatcher<byte>(bink.BaseAddress + 0x1b068);
            _watcher.Add(_isInCutscene);
        }

        public override void OnTimerReset(bool resetFlagTo)
        {
            _onceFlag = false;
            _resetFlag = resetFlagTo;
            if (!resetFlagTo)
                _tutResetFlag = true;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsLastMap)
            {
                _playerHP = new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset);
                _watcher.Add(_playerHP);
            }
            else if (IsFirstMap2)
            {
                _playerLaggedMovementValue = new MemoryWatcher<float>(state.PlayerEntInfo.EntityPtr + _basePlayerLaggedMovementOffset);
                _playerLaggedMovementValue.Update(state.GameProcess);

                _blockerIndex = state.GetEntIndexByName("blocker");
                Debug.WriteLine("blocker entity index is " + _blockerIndex);
            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            _watcher.UpdateAll(state.GameProcess);

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (state.CurrentMap.ToLower() == "a0c0p1" && state.PlayerPosition.DistanceXY(_startPos) <= 3f)
            {
                bool ifIntroNotDeleted = File.Exists(state.GameProcess.ReadString(state.GameOffsets.GameDirPtr, 255) + "/media/a0b0c0s0.bik");
                if (_tutResetFlag && 
                    (ifIntroNotDeleted && _isInCutscene.Current - _isInCutscene.Old == -1) ^ 
                    (!ifIntroNotDeleted && !_resetFlag && state.TickCount <= 1 && state.RawTickCount <= 150))
                {
                    Debug.WriteLine("hdtf start");
                    _onceFlag = true;
                    _resetFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsFirstMap2)
            {
                _playerLaggedMovementValue.Update(state.GameProcess);

                if (_playerLaggedMovementValue.Current == 1.0f && _playerLaggedMovementValue.Old == 0f)
                {
                    Debug.WriteLine("hdtf tutorial start");
                    return GameSupportResult.PlayerGainedControl;
                }

                IntPtr blockerNew = state.GetEntInfoByIndex(_blockerIndex).EntityPtr;
                if (blockerNew == IntPtr.Zero && _blockerIndex != -1)
                {
                    _onceFlag = true;
                    _blockerIndex = -1; 
                    Debug.WriteLine("hdtf tutorial end");
                    _tutResetFlag = false;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            else if (this.IsLastMap)
            {
                if (_playerHP.Old > 0 && _playerHP.Current <= 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("hdtf end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

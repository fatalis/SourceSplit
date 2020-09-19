using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HDTF : GameSupport
    {
        // start: on loading first map OR if the cutscene finishes playing out
        // ending: when the blocker brush entity is killed

        private bool _onceFlag;
        private static bool _resetFlag;
        private static bool _resetFlag2;

        private int _blocker_Index;
        private int _baseEntityHealthOffset = -1;
        private Vector3f _startPos = new Vector3f(772f, -813f, 164f);
        private Vector3f _tutStartPos = new Vector3f(-1105f, 5845f, 37f);

        private MemoryWatcher<int> _playerHP;
        private MemoryWatcher<byte> _isInCutscene;
        private MemoryWatcherList _watcher = new MemoryWatcherList();

        public HDTF()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "a0c0p1";
            this.FirstMap2 = "a0c0p0"; // boot camp
            this.LastMap = "a4c1p2";
            this.RequiredProperties = PlayerProperties.ALL;
        }
        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            ProcessModuleWow64Safe bink = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "video_bink.dll");
          
            Trace.Assert(server != null && bink != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));

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
            _resetFlag2 = resetFlagTo;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsLastMap)
            {
                _playerHP = new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset);
                _watcher.Add(_playerHP);
            }
            if (IsFirstMap2)
            {
                _blocker_Index = state.GetEntIndexByName("blocker");
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            _watcher.UpdateAll(state.GameProcess);

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            // this code is hacky but the starting conditions do not help
            if (this.IsFirstMap && state.PlayerPosition.DistanceXY(_startPos) <= 3f)
            {
                if (_isInCutscene.Changed && _isInCutscene.Old == 1 && _isInCutscene.Current == 0)
                {
                    Debug.WriteLine("hdtf start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }

                // only check if the map has loaded in and the output to play the cutscene is fired
                else if (_isInCutscene.Current == 0 && !_resetFlag && state.TickCount >= 10 && state.TickCount <= 40)
                {
                    Debug.WriteLine("hdtf start");
                    _onceFlag = true;
                    _resetFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }

            }
            else if (this.IsFirstMap2)
            { 
                if (state.PlayerPosition.DistanceXY(_tutStartPos) >= 0.1f &&
                    state.PrevPlayerPosition.DistanceXY(_tutStartPos) < 0.1f && !_resetFlag2)
                {
                    _resetFlag2 = true;
                    Debug.WriteLine("hdtf tutorial start");
                    return GameSupportResult.PlayerGainedControl;
                }

                IntPtr blockerNew = state.GetEntInfoByIndex(_blocker_Index).EntityPtr;
                if (blockerNew == IntPtr.Zero)
                {
                    _onceFlag = true;
                    Debug.WriteLine("hdtf tutorial end");
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

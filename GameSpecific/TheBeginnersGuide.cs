using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TheBeginnersGuide : GameSupport
    {
        // start: 2:27.50 before map load 
        // ending: when player move speed is modified

        private bool _onceFlag = false;

        private MemoryWatcher<float> _playerMoveSpeed;

        public TheBeginnersGuide()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.StartOffsetTicks = -8850;
            this.AddFirstMap("whisper");
            this.AddLastMap("nomansland2");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server;
            server = state.GetModule("server.dll");
            _playerMoveSpeed = new MemoryWatcher<float>(server.BaseAddress + 0x761310);
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
                _playerMoveSpeed.Update(state.GameProcess);
                if (_playerMoveSpeed.Old != 0 && _playerMoveSpeed.Current == 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("tbg end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TheBeginnersGuide : GameSupport
    {
        // start: 2:27.50 before map load 
        // ending: when final trigger_once is triggered (in other words, killed)

        private bool _onceFlag = false;

        private MemoryWatcher<float> _playerMoveSpeed;
        private const int _brushDisabledOffset = 864;

        public TheBeginnersGuide()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.StartOffsetTicks = -8850;
            this.StartOnFirstMapLoad = true;
            this.FirstMap = "whisper";
            this.LastMap = "nomansland2";
            this.RequiredProperties = PlayerProperties.Position;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server;
            server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

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

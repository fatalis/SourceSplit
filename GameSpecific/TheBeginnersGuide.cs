using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TheBeginnersGuide : GameSupport
    {
        // start: 2:27.50 seconds after loading the 2nd level (when intro blocking brush is killed on map start)
        // ending: when final trigger_once is triggered (in other words, killed)

        private bool _onceFlag = false;

        private MemoryWatcher<byte> _blockBrushDisabled;
        private MemoryWatcher<float> _playerMoveSpeed;
        private const int _brushDisabledOffset = 864;

        public TheBeginnersGuide()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
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
            if (this.IsFirstMap)
            {
                this._blockBrushDisabled = new MemoryWatcher<byte>(state.GetEntityByName("intro_block") + _brushDisabledOffset);
            }
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                _blockBrushDisabled.Update(state.GameProcess);

                if (_blockBrushDisabled.Old == 0 && _blockBrushDisabled.Current == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("tbg start");
                    this.StartOffsetTicks = -8850;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
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

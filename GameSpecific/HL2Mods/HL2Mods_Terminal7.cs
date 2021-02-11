using LiveSplit.ComponentUtil;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Terminal7 : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the game begins fading out

        private bool _onceFlag;

        private MemoryWatcher<int> _fadeListSize;

        public HL2Mods_Terminal7()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "t7_01";
            this.LastMap = "t7_cr";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnGameAttached(GameState state)
        {
            base.OnGameAttached(state);
            _fadeListSize = new MemoryWatcher<int>(state.GameOffsets.FadeListPtr + 0x10);
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
                _fadeListSize.Update(state.GameProcess);

                float splitTime = state.FindFadeEndTime(-2560f, 0, 0, 0);

                if (splitTime != 0f && _fadeListSize.Old == 0 && _fadeListSize.Current == 1)
                {
                    Debug.WriteLine("terminal 7 end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

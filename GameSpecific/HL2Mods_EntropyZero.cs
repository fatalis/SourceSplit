using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_EntropyZero : GameSupport
    {
        // how to match with demos:
        // start: on first map load
        // ending: when the final logic_relay is triggered

        private bool _onceFlag;

        public HL2Mods_EntropyZero()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "az_c1_1";
            this.LastMap = "az_c4_3";
            this.StartOnFirstMapLoad = true;
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

            if (IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("STASIS_SEQ_LazyGo", 3);
                if (splitTime != 0f && Math.Abs(splitTime - state.RawTickCount * state.IntervalPerTick) <= 0.05f)
                {
                    _onceFlag = true;
                    Debug.WriteLine("entropy zero end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

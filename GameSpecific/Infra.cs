using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Infra : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // endings: all on fades

        private bool _onceFlag = false;

        public Infra()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "infra_c1_m1_office";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public GameSupportResult DefaultEnd(GameState state, float fadeSpeed, string ending)
        {
            float splitTime = state.FindFadeEndTime(fadeSpeed);
            // this is how the game actually knows when a fade has finished as well
            if (state.CompareToInternalTimer(splitTime, 0.05f))
            {
                _onceFlag = true;
                Debug.WriteLine("infra " + ending + " ending");
                return GameSupportResult.ManualSplit;
            }
            else
                return GameSupportResult.DoNothing;
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

            switch (state.CurrentMap.ToLower())
            {
                default:
                    return GameSupportResult.DoNothing;

                case "infra_c5_m2b_sewer2":
                    {
                        return DefaultEnd(state, -2560f, "part 1");
                    }
                case "infra_c7_m5_powerstation":
                    {
                        // v2 and v3 have different start and end durations since v2 ends in a credits sequence 
                        var test = DefaultEnd(state, -85f, "part 2");
                        if (test == GameSupportResult.DoNothing)
                            return DefaultEnd(state, -2560f, "part 2");
                        else
                            return test;
                    }
                case "infra_c11_ending_1":
                case "infra_c11_ending_2":
                case "infra_c11_ending_3":
                    {
                        return DefaultEnd(state, -25.5f, "part 3");
                    }
            }
        }
    }
}

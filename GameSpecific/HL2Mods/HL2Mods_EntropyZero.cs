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
        private float _splitTime;

        public HL2Mods_EntropyZero()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "az_c1_1";
            this.LastMap = "az_c4_3";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsLastMap)
                _splitTime = state.FindOutputFireTime("STASIS_SEQ_LazyGo", 3);

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsLastMap)
            {
                float newSplitTime = state.FindOutputFireTime("STASIS_SEQ_LazyGo", 3);
                if (newSplitTime == 0f && _splitTime != 0f)
                {
                    _onceFlag = true;
                    Debug.WriteLine("entropy zero end");
                    return GameSupportResult.PlayerLostControl;
                }
                _splitTime = newSplitTime;
            }

            return GameSupportResult.DoNothing;
        }
    }
}

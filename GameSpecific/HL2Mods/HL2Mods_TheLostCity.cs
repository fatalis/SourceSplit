using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_TheLostCity : GameSupport
    {
        // start: on first map
        // ending: when the gunship dies and queues final output

        private bool _onceFlag;
        private float _splitTime;

        public HL2Mods_TheLostCity()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "lostcity01";
            this.LastMap = "lostcity02";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap)
                _splitTime = state.FindOutputFireTime("fade1", "fade", "", 3);

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float newSplitTime = state.FindOutputFireTime("fade1", "fade", "" , 3);

                if (newSplitTime != 0 && _splitTime == 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("the lost city end");
                    return GameSupportResult.PlayerLostControl;
                }

                _splitTime = newSplitTime;
            }

            return GameSupportResult.DoNothing;
        }
    }
}

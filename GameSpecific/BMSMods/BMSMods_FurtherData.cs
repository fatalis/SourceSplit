using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class BMSMods_FurtherData : GameSupport
    {
        // how to match with demos:
        // start: on first map
        // end: when the final output is queued 

        private bool _onceFlag = false;
        private float _splitTime;

        public BMSMods_FurtherData()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "fd01";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsFirstMap)
                _splitTime = state.FindOutputFireTime("end_btd_sd", "PlaySound", "", 6);

            _onceFlag = false;

        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("end_btn_sd", "PlaySound", "", 6);
                if (splitTime != 0f && _splitTime == 0f)
                {
                    Debug.WriteLine("fd end");
                    _splitTime = splitTime; 
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
                _splitTime = splitTime;
            }

            return GameSupportResult.DoNothing;
        }
    }
}
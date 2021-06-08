using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_GetALife : GameSupport
    {
        // start: when the view entity switches to the player
        // ending: when the view entity switches from the player to the final cam entity

        private bool _onceFlag;

        private int _startCamIndex;
        private int _endCamIndex;

        public HL2Mods_GetALife()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "boulevard";
            this.LastMap = "labo2";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsFirstMap)
            {
                _startCamIndex = state.GetEntIndexByName("point_viewcontrolintro");
                Debug.WriteLine("found start cam at " + _startCamIndex);
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GetEntIndexByName("point_viewcontrol_finboss1");
                Debug.WriteLine("found end cam at " + _endCamIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex == 1 && state.PrevPlayerViewEntityIndex == _startCamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("get a life start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PrevPlayerViewEntityIndex == 1 && state.PlayerViewEntityIndex == _endCamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("get a life end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

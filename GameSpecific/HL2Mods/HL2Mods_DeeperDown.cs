using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DeeperDown : GameSupport
    {
        // how to match with demos:
        // start: when the view entity switches back to the player
        // ending: when the output to the final relay is fired

        private bool _onceFlag;

        private int _camIndex;
        private float _splitTime;

        public HL2Mods_DeeperDown()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ep2_dd2_1";
            this.LastMap = "ep2_dd2_9";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                this._camIndex = state.GetEntIndexByName("PointViewCont1");
                Debug.WriteLine("_camIndex index is " + this._camIndex);
            }
            else if (this.IsLastMap)
            {
                _splitTime = state.FindOutputFireTime("OW_Dead_Relay", 2);
            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex == 1 && state.PrevPlayerViewEntityIndex == _camIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("deeper down start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap)
            {
                float newSplitTime = state.FindOutputFireTime("OW_Dead_Relay", 2);
                if (_splitTime != 0f && newSplitTime == 0f)
                {
                    Debug.WriteLine("deeper down end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
                _splitTime = newSplitTime;
            }
            return GameSupportResult.DoNothing;
        }
    }
}

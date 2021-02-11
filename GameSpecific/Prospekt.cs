using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Prospekt : GameSupport
    {
        // how to match with demos:
        // start: when the view entity switches to the player
        // endings: when the view entity switches to the final camera

        private bool _onceFlag = false;

        private int _startCamIndex;

        private int _endCamIndex;

        public Prospekt()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "pxg_level_01_fg";
            this.LastMap = "pxg_finallevel01a";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;

            if (IsFirstMap)
            {
                _startCamIndex = state.GetEntIndexByName("secondary_camera");
                Debug.WriteLine("found start cam at " + _startCamIndex);
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GetEntIndexByName("background_camera1");
                Debug.WriteLine("found end cam at " + _endCamIndex);
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                if (state.PlayerViewEntityIndex == 1 && state.PrevPlayerViewEntityIndex == _startCamIndex)
                {
                    Debug.WriteLine("prospekt start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (IsLastMap)
            {
                if (state.PlayerViewEntityIndex == _endCamIndex && state.PrevPlayerViewEntityIndex == 1)
                {
                    Debug.WriteLine("prospekt end");
                    _onceFlag = false;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

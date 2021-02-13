using LiveSplit.ComponentUtil;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Precursor : GameSupport
    {
        // start: when the view entity switches to the player from the start cam
        // ending: when the view entity switches to the end cam from the player

        private bool _onceFlag;

        private int _startCamIndex;
        private int _endCamIndex;

        public HL2Mods_Precursor()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "r_map1";
            this.LastMap = "r_map7";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsFirstMap)
            {
                _startCamIndex = state.GetEntIndexByName("camera2_camera");
                Debug.WriteLine("found start cam index at " + _startCamIndex);
            }
            else if (IsLastMap)
            {
                _endCamIndex = state.GetEntIndexByName("end_lockplayer");
                Debug.WriteLine("found end cam index at " + _endCamIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex == _startCamIndex && state.PlayerViewEntityIndex == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("precursor start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PrevPlayerViewEntityIndex == 1 && state.PlayerViewEntityIndex == _endCamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("precursor end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

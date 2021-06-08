using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Grey : GameSupport
    {
        // start: when the view entity switches from the start cam to the player
        // ending: when the view entity switches from the player to the end cam

        private bool _onceFlag;
        
        private int _startcamIndex;

        private int _endcamIndex;

        public HL2Mods_Grey()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "map0";
            this.LastMap = "map11";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsFirstMap)
            {
                _startcamIndex = state.GetEntIndexByName("asd2");
                Debug.WriteLine("found start cam at " + _startcamIndex);
            }
            else if (IsLastMap)
            {
                _endcamIndex = state.GetEntIndexByName("camz1");
                Debug.WriteLine("found end cam at " + _endcamIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex == _startcamIndex && state.PlayerViewEntityIndex == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("grey start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PrevPlayerViewEntityIndex == 1 && state.PlayerViewEntityIndex == _endcamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("grey end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

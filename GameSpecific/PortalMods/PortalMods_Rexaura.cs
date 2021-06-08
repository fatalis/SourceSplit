using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_Rexaura : GameSupport
    {
        // how to match this timing with demos:
        // start: on view entity changing to the player's
        // ending: on view entity changing from the player to final camera

        private int _startCamIndex;
        private int _endCamIndex;
        private bool _onceFlag;

        public PortalMods_Rexaura()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "rex_00_intro";
            this.LastMap = "rex_19_remote";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                _startCamIndex = state.GetEntIndexByName("wub_viewcontrol");
                Debug.WriteLine("found start cam index at " + _startCamIndex);
            }
            else if (this.IsLastMap)
            {
                _endCamIndex = state.GetEntIndexByName("end_game_camera");
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
                    Debug.WriteLine("rexaura start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PrevPlayerViewEntityIndex == 1 && state.PlayerViewEntityIndex == _endCamIndex)
                {
                    Debug.WriteLine("rexaura end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }

    }
}

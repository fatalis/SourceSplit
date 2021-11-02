
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_EpicEdition : GameSupport
    {
        // how to match this timing with demos:
        // start: when view entity changes from the camera's
        // ending: (achieved using map transition)

        private int _startCamIndex;
        private bool _onceFlag;

        public PortalMods_EpicEdition()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("pee_chmb_00");
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsFirstMap)
            {
                _startCamIndex = state.GetEntIndexByName("blackout_viewcontroller");
                Debug.WriteLine($"start cam idex is {_startCamIndex}");
            }
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex == _startCamIndex && state.PlayerViewEntityIndex == 1)
                    return GameSupportResult.PlayerGainedControl;
            }

            return GameSupportResult.DoNothing;
        }

    }
}

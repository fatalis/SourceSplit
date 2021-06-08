using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Survivor : GameSupport
    {
        // start: on loading the first map (game begins by loading a pre-made save)
        // ending: when the player's view entity switches to the final

        private bool _onceFlag;
        private bool _startFlag;
        private float _splitTime;
        private const int DEFAULT_START_SAVE_TICK = 1670;

        private int _finalCamIndex;

        public HL2Survivor()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicks;
            this.FirstMap = "chapter01_1";
            this.LastMap = "chapter10_5";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap)
            {
                _finalCamIndex = state.GetEntIndexByName("cam02");
                Debug.WriteLine("found final camera's entity index at " + _finalCamIndex);
            }

            _onceFlag = false;
            _startFlag = false;
            _splitTime = 0f;
        }

        public override void OnGenericUpdate(GameState state)
        {
            if (_onceFlag) 
                return;

            float splitTime = state.FindOutputFireTime("", "NextScene", "", 7, true, false);
            if (splitTime == 0f)
                splitTime = state.FindOutputFireTime("", "StageClear", "", 7, true, false);
            _splitTime = (splitTime == 0f) ? _splitTime : splitTime;

            if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
            {
                this.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                _splitTime = 0f;
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && !_startFlag)
            {
                if (state.TickBase >= DEFAULT_START_SAVE_TICK 
                    && state.TickBase <= DEFAULT_START_SAVE_TICK + 10)
                {
                    // can't have onceflag here as it'd negate the splitting code on this map
                    _startFlag = true;
                    Debug.WriteLine("hl2 survivor start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                if (state.PrevPlayerViewEntityIndex == 1 && state.PlayerViewEntityIndex == _finalCamIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("hl2 survivor end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

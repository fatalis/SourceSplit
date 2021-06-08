using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_UncertaintyPrinciple : GameSupport
    {
        // start: when the player's view entity index changes back to the player's
        // ending: when player is frozen by the camera entity

        private bool _onceFlag;
        private int _camIndex;

        public HL2Mods_UncertaintyPrinciple()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "up_retreat_a";
            this.LastMap = "up_night";
            this.RequiredProperties = PlayerProperties.Flags;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                _camIndex = state.GetEntIndexByName("camera1");
                Debug.WriteLine("start camera index is " + _camIndex);
            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex == _camIndex && state.PlayerViewEntityIndex == 1)
                {
                    Debug.WriteLine("up start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                if (!state.PrevPlayerFlags.HasFlag(FL.FROZEN) && state.PlayerFlags.HasFlag(FL.FROZEN))
                {
                    Debug.WriteLine("up end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_TheCitizen2 : GameSupport
    {
        // start: when the output to give the player the suit is fired AND a trigger in the level has not been hit
        // ending: when the end text model's skin code is 10 and player view entity switches to the final camera

        private bool _onceFlag;

        private int _trigIndex;

        public HL2Mods_TheCitizen2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "sp_intro";
            this.LastMap = "sp_square";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsFirstMap)
            {
                _trigIndex = state.GetEntIndexByPos(-1973f, -4511f, -1901.5f);
                Debug.WriteLine("target trigger found at " + _trigIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("commander", "Command", "give item_suit", 4);
                IntPtr trigPtr = state.GetEntInfoByIndex(_trigIndex).EntityPtr;
                if (trigPtr != IntPtr.Zero && state.CompareToInternalTimer(splitTime))
                {
                    _onceFlag = true;
                    Debug.WriteLine("the citizen 2 start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.FindFadeEndTime(-2560f);
                if (state.CompareToInternalTimer(splitTime))
                {
                    _onceFlag = true;
                    Debug.WriteLine("the citizen 2 end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

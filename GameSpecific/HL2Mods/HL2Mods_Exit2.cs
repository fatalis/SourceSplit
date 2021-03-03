using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Exit2 : GameSupport
    {
        // start: when player's view index changes from the camera entity to the player
        // ending: when the final trigger_once is hit and the fade finishes

        private bool _onceFlag = false;

        private int _camIndex;
        private int _trigIndex;

        public HL2Mods_Exit2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "e2_01";
            this.LastMap = "e2_07";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._camIndex = state.GetEntIndexByName("view");
                Debug.WriteLine("_camIndex index is " + this._camIndex);
            }

            if (this.IsLastMap)
            {
                this._trigIndex = state.GetEntIndexByPos(-840f, -15096f, 48f);
                Debug.WriteLine("_trigIndex index is " + this._trigIndex);

            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && this._camIndex != -1)
            {
                if (state.PlayerViewEntityIndex == 1 &&
                    state.PrevPlayerViewEntityIndex == _camIndex)
                {
                    Debug.WriteLine("exit2 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                var newTrig = state.GetEntInfoByIndex(_trigIndex);
                float splitTime = state.FindFadeEndTime(-127.5f);

                if (state.CompareToInternalTimer(splitTime, 0f, true))
                {
                    Debug.WriteLine("exit2 end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

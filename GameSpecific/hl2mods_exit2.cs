using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_exit2 : GameSupport
    {
        // start: when player's view index changes from the camera entity to the player
        // ending: 2 seconds after final trigger_once is hit and fade starts

        private bool _onceFlag = false;

        private int _cam_index;
        private int _trig_index;

        public hl2mods_exit2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "e2_01"; //beta%
            this.LastMap = "e2_07";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._cam_index = state.GetEntIndexByName("view");
                Debug.WriteLine("_cam_index index is " + this._cam_index);
            }

            if (this.IsLastMap)
            {
                this._trig_index = state.GetEntIndexByPos(-840f, -15096f, 48f);
                Debug.WriteLine("_trig_index index is " + this._trig_index);

            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && this._cam_index != -1)
            {
                if (state.PlayerViewEntityIndex == 1 &&
                    state.PrevPlayerViewEntityIndex == _cam_index)
                {
                    Debug.WriteLine("exit2 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && _trig_index != -1)
            {
                var newtrig = state.GetEntInfoByIndex(_trig_index);

                if (newtrig.EntityPtr == IntPtr.Zero)
                {
                    Debug.WriteLine("exit2 end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -134;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}
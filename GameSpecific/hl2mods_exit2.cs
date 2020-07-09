using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Exit2 : GameSupport
    {
        // start: when player's view index changes from the camera entity to the player
        // ending: 2 seconds after final trigger_once is hit and fade starts

        private bool _onceFlag = false;

        private int _cam_Index;
        private int _trig_Index;

        public HL2Mods_Exit2()
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
                this._cam_Index = state.GetEntIndexByName("view");
                Debug.WriteLine("_cam_Index index is " + this._cam_Index);
            }

            if (this.IsLastMap)
            {
                this._trig_Index = state.GetEntIndexByPos(-840f, -15096f, 48f);
                Debug.WriteLine("_trig_Index index is " + this._trig_Index);

            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && this._cam_Index != -1)
            {
                if (state.PlayerViewEntityIndex == 1 &&
                    state.PrevPlayerViewEntityIndex == _cam_Index)
                {
                    Debug.WriteLine("exit2 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && _trig_Index != -1)
            {
                var newTrig = state.GetEntInfoByIndex(_trig_Index);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    Debug.WriteLine("exit2 end");
                    _onceFlag = true;
                    this.EndOffsetTicks = -127;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}
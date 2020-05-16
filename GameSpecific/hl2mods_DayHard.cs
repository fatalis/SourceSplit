using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_DayHard : GameSupport
    {
        // start: when player view entity changes from start camera to the player
        // ending: when breen is killed

        private bool _onceFlag = false;

        private int _cam_index;
        private int _prop_index;

        public hl2mods_DayHard()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "dayhardpart1";
            this.LastMap = "breencave";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._cam_index = state.GetEntIndexByName("cutscene3");
                Debug.WriteLine("_cam_index index is " + this._cam_index);
            }

            if (this.IsLastMap)
            {
                this._prop_index = state.GetEntIndexByName("Patch3");
                Debug.WriteLine("_prop_index index is " + this._prop_index);
            }
            _onceFlag = false; 
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _cam_index != -1)
            {
                if (state.PlayerViewEntityIndex == 1 &&
                    state.PrevPlayerViewEntityIndex == _cam_index)
                {
                    Debug.WriteLine("DayHard start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }

            }

            else if (this.IsLastMap && _prop_index != -1)
            {
                var newprop = state.GetEntInfoByIndex(_prop_index);

                if (newprop.EntityPtr == IntPtr.Zero)
                {
                    Debug.WriteLine("DayHard end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}
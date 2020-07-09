using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DayHard : GameSupport
    {
        // start: when player view entity changes from start camera to the player
        // ending: when breen is killed

        private bool _onceFlag = false;

        private int _cam_Index;
        private int _prop_Index;

        public HL2Mods_DayHard()
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
                this._cam_Index = state.GetEntIndexByName("cutscene3");
                Debug.WriteLine("_cam_Index index is " + this._cam_Index);
            }

            if (this.IsLastMap)
            {
                this._prop_Index = state.GetEntIndexByName("Patch3");
                Debug.WriteLine("_prop_Index index is " + this._prop_Index);
            }
            _onceFlag = false; 
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _cam_Index != -1)
            {
                if (state.PlayerViewEntityIndex == 1 &&
                    state.PrevPlayerViewEntityIndex == _cam_Index)
                {
                    Debug.WriteLine("DayHard start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }

            }

            else if (this.IsLastMap && _prop_Index != -1)
            {
                var newProp = state.GetEntInfoByIndex(_prop_Index);

                if (newProp.EntityPtr == IntPtr.Zero)
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

using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DayHard : GameSupport
    {
        // start: when player view entity changes from start camera to the player
        // ending: when breen is killed

        private bool _onceFlag = false;

        private int _camIndex;
        private int _propIndex;

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
                this._camIndex = state.GetEntIndexByName("cutscene3");
                Debug.WriteLine("_camIndex index is " + this._camIndex);
            }

            if (this.IsLastMap)
            {
                this._propIndex = state.GetEntIndexByName("Patch3");
                Debug.WriteLine("_propIndex index is " + this._propIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex == 1 &&
                    state.PrevPlayerViewEntityIndex == _camIndex)
                {
                    Debug.WriteLine("DayHard start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }

            }

            else if (this.IsLastMap && _propIndex != -1)
            {
                var newProp = state.GetEntInfoByIndex(_propIndex);

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

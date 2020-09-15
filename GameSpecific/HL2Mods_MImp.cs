using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_MImp : GameSupport
    {
        // how to match with demos:
        // start: 1s (~67 ticks after the timer starts) after cave_giveitems_trig is triggered
        // ending: when player's view entity changes

        private bool _onceFlag;

        private int _trigIndex;
        private int _camIndex;

        public HL2Mods_MImp()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "mimp1";
            this.LastMap = "mimp3";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                this._trigIndex = state.GetEntIndexByName("cave_giveitems_trig");
                Debug.WriteLine("cave_giveitems_trig index is " + this._trigIndex);
            }

            if (this.IsLastMap)
            {
                this._camIndex = state.GetEntIndexByName("outro.camera");
                Debug.WriteLine("_camIndex index is " + this._camIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && this._trigIndex != -1)
            {
                var newTrig = state.GetEntInfoByIndex(_trigIndex);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    _trigIndex = -1;
                    Debug.WriteLine("mimp start");
                    this.StartOffsetTicks = 62;
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex == _camIndex && state.PrevPlayerViewEntityIndex != _camIndex)
                {
                    Debug.WriteLine("mimp end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

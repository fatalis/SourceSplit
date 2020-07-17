using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_MImp : GameSupport
    {
        // how to match with demos:
        // start: 1s (~67 ticks after the timer starts) after cave_giveitems_trig is triggered
        // ending: when player's view entity changes

        private const int ENT_INDEX_PLAYER = 1;
        private bool _onceFlag;

        private int _trig_Index;
        private int _cam_Index;

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
                this._trig_Index = state.GetEntIndexByName("cave_giveitems_trig");
                Debug.WriteLine("cave_giveitems_trig index is " + this._trig_Index);
            }

            if (this.IsLastMap)
            {
                this._cam_Index = state.GetEntIndexByName("outro.camera");
                Debug.WriteLine("_cam_Index index is " + this._cam_Index);
            }
                _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
            {
                return GameSupportResult.DoNothing;
            }

            if (this.IsFirstMap && this._trig_Index != -1)
            {
                var newTrig = state.GetEntInfoByIndex(_trig_Index);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    _trig_Index = -1;
                    Debug.WriteLine("mimp start");
                    this.StartOffsetTicks = 62;
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap && _cam_Index != -1)
            {
                if (state.PlayerViewEntityIndex == _cam_Index)
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

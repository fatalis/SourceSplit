using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_mimp : GameSupport
    {
        // how to match with demos:
        // start: 1s (~67 ticks after the timer starts) after cave_giveitems_trig is triggered
        // ending: when player's view entity changes

        private const int ENT_INDEX_PLAYER = 1;
        private bool _onceFlag;

        private int _trig_index;

        public hl2mods_mimp()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "mimp1";
            this.LastMap = "mimp3";
            this.RequiredProperties = PlayerProperties.Position;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                this._trig_index = state.GetEntIndexByName("cave_giveitems_trig");
                this.StartOffsetTicks = 62;
                Debug.WriteLine("cave_giveitems_trig index is " + this._trig_index);
            }

                _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
            {
                return GameSupportResult.DoNothing;
            }

            if (this.IsFirstMap && this._trig_index != -1)
            {
                var newtrig = state.GetEntInfoByIndex(_trig_index);

                if (newtrig.EntityPtr == IntPtr.Zero)
                {
                    _trig_index = -1;
                    Debug.WriteLine("mimp start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap)
            {
                if (state.PlayerViewEntityIndex != ENT_INDEX_PLAYER)
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
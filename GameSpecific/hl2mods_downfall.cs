using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_downfall : GameSupport
    {
        // start: when player view entity changes
        // ending: when elevator button is pressed

        private const int ENT_INDEX_PLAYER = 1;
        private bool _onceFlag;

        private int _sprite_index;

        public hl2mods_downfall()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "dwn01";
            this.LastMap = "dwn01a";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._sprite_index = state.GetEntIndexByName("elevator02_button_sprite");
                Debug.WriteLine("elevator02_button_sprite index is " + this._sprite_index);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
            {
                return GameSupportResult.DoNothing;
            }

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex != GameState.ENT_INDEX_PLAYER
                    && state.PlayerViewEntityIndex == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("downfall start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap && _sprite_index != 1)
            {
                var newblack = state.GetEntInfoByIndex(_sprite_index);

                if (newblack.EntityPtr == IntPtr.Zero)
                {
                    _sprite_index = -1;
                    Debug.WriteLine("downfall end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
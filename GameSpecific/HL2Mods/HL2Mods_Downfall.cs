using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Downfall : GameSupport
    {
        // start: when player view entity changes
        // ending: when elevator button is pressed

        private bool _onceFlag;

        private int _spriteIndex;

        public HL2Mods_Downfall()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "dwn01";
            this.LastMap = "dwn01a";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._spriteIndex = state.GetEntIndexByName("elevator02_button_sprite");
                Debug.WriteLine("elevator02_button_sprite index is " + this._spriteIndex);
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

            else if (this.IsLastMap && _spriteIndex != 1)
            {
                var newBlack = state.GetEntInfoByIndex(_spriteIndex);

                if (newBlack.EntityPtr == IntPtr.Zero)
                {
                    _spriteIndex = -1;
                    Debug.WriteLine("downfall end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

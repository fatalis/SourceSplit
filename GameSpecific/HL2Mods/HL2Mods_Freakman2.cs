using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Freakman2 : GameSupport
    {
        // start: when player gains control from camera entity (when the its parent entity is killed)
        // ending: when player's view entity changes to the ending camera

        private bool _onceFlag;

        private int _trainIndex;
        private int _camIndex;

        public HL2Mods_Freakman2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "kleiner0";
            this.LastMap = "thestoryhappyend";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                _trainIndex = state.GetEntIndexByName("lookatthis_move");
                Debug.WriteLine("camera parent entity index is " + _trainIndex);
            }
            _onceFlag = false;

            if (this.IsLastMap)
            {
                _camIndex = state.GetEntIndexByName("credit_cam");
                Debug.WriteLine("cam index is " + _camIndex);
            }
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _trainIndex != -1)
            {
                var newTrig = state.GetEntInfoByIndex(_trainIndex);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    _trainIndex = -1;
                    _onceFlag = true;
                    this.StartOffsetTicks = -4;
                    Debug.WriteLine("freakman2 start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap && _camIndex != -1)
            {
                if (state.PrevPlayerViewEntityIndex != _camIndex && state.PlayerViewEntityIndex == _camIndex)
                {
                    _onceFlag = true;
                    Debug.WriteLine("freakman2 end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

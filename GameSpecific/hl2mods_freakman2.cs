using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_freakman2 : GameSupport
    {
        // start: when player gains control from camera entity (when the its parent entity is killed)
        // ending: when player's view entity changes to the ending camera

        private bool _onceFlag;

        private int train_index;
        private int cam_index;

        public hl2mods_freakman2()
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
                train_index = state.GetEntIndexByName("lookatthis_move");
            }
            _onceFlag = false;

            if (this.IsLastMap)
            {
                cam_index = state.GetEntIndexByName("credit_cam");
                Debug.WriteLine("cam index is " + cam_index);
            }
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && train_index != -1)
            {
                var newtrig = state.GetEntInfoByIndex(train_index);
                
                if (newtrig.EntityPtr == IntPtr.Zero)
                {
                    train_index = -1;
                    _onceFlag = true;
                    this.StartOffsetTicks = -4;
                    Debug.WriteLine("freakman2 start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap && cam_index != -1)
            {
                if (state.PlayerViewEntityIndex == cam_index)
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
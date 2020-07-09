using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Ptsd1 : GameSupport
    {
        // how to match with demos:
        // start: after player view entity changes (requires debug config and checks to avoid other changes)
        // ending: when breen's banana hat (yes really) is killed

        private bool _onceFlag;

        private int _breen_Index;
        private int _cam_Index;

        public HL2Mods_Ptsd1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ptsd_1";
            this.LastMap = "ptsd_final";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                this._cam_Index = state.GetEntIndexByName("camera_1");
            }

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._breen_Index = state.GetEntIndexByName("banana2");
                Debug.WriteLine("banana2 index is " + this._breen_Index);
            }
        
             _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _cam_Index != -1)
            {
                if (state.PrevPlayerViewEntityIndex == _cam_Index
                    && state.PlayerViewEntityIndex == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("ptsd start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && this._breen_Index != -1)
            {
                var newBlack = state.GetEntInfoByIndex(_breen_Index);

                if (newBlack.EntityPtr == IntPtr.Zero)
                {
                    _breen_Index = -1;
                    Debug.WriteLine("ptsd end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

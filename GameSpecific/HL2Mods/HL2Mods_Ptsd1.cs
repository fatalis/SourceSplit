using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Ptsd1 : GameSupport
    {
        // how to match with demos:
        // start: after player view entity changes
        // ending: when breen's banana hat (yes really) is killed

        private bool _onceFlag;

        private int _breenIndex;
        private int _camIndex;

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
                this._camIndex = state.GetEntIndexByName("camera_1");
                Debug.WriteLine("start cam index is " + _camIndex);
            }

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._breenIndex = state.GetEntIndexByName("banana2");
                Debug.WriteLine("banana2 index is " + this._breenIndex);
            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _camIndex != -1)
            {
                if (state.PrevPlayerViewEntityIndex == _camIndex
                    && state.PlayerViewEntityIndex == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("ptsd start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && this._breenIndex != -1)
            {
                var newBlack = state.GetEntInfoByIndex(_breenIndex);

                if (newBlack.EntityPtr == IntPtr.Zero)
                {
                    _breenIndex = -1;
                    Debug.WriteLine("ptsd end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

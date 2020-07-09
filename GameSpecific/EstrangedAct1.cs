using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class EstrangedAct1 : GameSupport
    {
        // start: 6.8 seconds after a trigger_once is hit
        // ending: when final trigger_once is hit, breaking the floor

        private bool _onceFlag;

        private int _trig_Index;
        private int _trig2_Index;

        public EstrangedAct1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "sp01thebeginning";
            this.LastMap = "sp10thewarehouse";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap)
            {
                this._trig_Index = state.GetEntIndexByPos(1120f, -1278f, 1292f);
                Debug.WriteLine("trig index is " + this._trig_Index);
            }
            if (this.IsLastMap)
            {
                this._trig2_Index = state.GetEntIndexByPos(5240f, -7800f, -206f);
                Debug.WriteLine("trig2 index is " + this._trig_Index);

            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && this._trig_Index != -1)
            {
                var newtrig = state.GetEntInfoByIndex(_trig_Index);

                if (newtrig.EntityPtr == IntPtr.Zero)
                {
                    _trig_Index = -1;
                    Debug.WriteLine("estranged2 start");
                    this.StartOffsetTicks = 791;
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            if (this.IsLastMap && this._trig2_Index != -1)
            {
                var newtrig2 = state.GetEntInfoByIndex(_trig2_Index);

                if (newtrig2.EntityPtr == IntPtr.Zero)
                {
                    _trig2_Index = -1;
                    Debug.WriteLine("estranged1 end");
                    _onceFlag = true;
                    this.EndOffsetTicks = (int)Math.Ceiling(0.1f / state.IntervalPerTick);
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

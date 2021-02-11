using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class LostCoast : GameSupport
    {
        // how to match with demos:
        // start: 0.2 seconds (14 ticks before timer starts) before the blackout camera guide entity is killed
        // ending: when final trigger_once is triggered (in other words, killed)

        private bool _onceFlag = false;

        private int _blackIndex;
        private int _trigIndex;

        public LostCoast()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "hdrtest"; //beta%
            this.LastMap = "d2_lostcoast";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap || this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._blackIndex = state.GetEntIndexByName("blackout");
                Debug.WriteLine("blackout index is " + this._blackIndex);
                this._trigIndex = state.GetEntIndexByPos(1109.82f, 2952f, 2521.26f);
                Debug.WriteLine("test index is " + this._trigIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this._blackIndex != -1)
            {
                var newBlack = state.GetEntInfoByIndex(_blackIndex);

                if (newBlack.EntityPtr == IntPtr.Zero)
                {
                    _blackIndex = -1;
                    Debug.WriteLine("lostcoast start");
                    this.StartOffsetTicks = -14;
                    // no once flag because the end wont trigger otherwise
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this._trigIndex != -1)
            {
                var newTrig = state.GetEntInfoByIndex(_trigIndex);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    _trigIndex = -1;
                    Debug.WriteLine("lostcoast end");
                    _onceFlag = true;
                    this.EndOffsetTicks = 7;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DeepDown : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        private bool _onceFlag;

        private int _intro_Index;
        private int _trig_Index;

        public HL2Mods_DeepDown()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ep2_deepdown_1";
            this.LastMap = "ep2_deepdown_5";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._intro_Index = state.GetEntIndexByName("IntroCredits1");
                Debug.WriteLine("intro index is " + this._intro_Index);
            }

            if (this.IsLastMap)
            {
                this._trig_Index = state.GetEntIndexByName("AlyxWake1");
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && this._intro_Index != -1)
            {
                var newIntro = state.GetEntInfoByIndex(_intro_Index);

                if (newIntro.EntityPtr == IntPtr.Zero)
                {
                    _intro_Index = -1;
                    Debug.WriteLine("deepdown start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && this._trig_Index != -1)
            {
                var newTrig = state.GetEntInfoByIndex(_trig_Index);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    Debug.WriteLine("deepdown end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
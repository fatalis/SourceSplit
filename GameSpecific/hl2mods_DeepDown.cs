using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_DeepDown : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        private bool _onceFlag;

        private int _intro_index;
        private int _trig_index;

        public hl2mods_DeepDown()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ep2_deepdown_1";
            this.LastMap = "ep2_deepdown_5";
            this.RequiredProperties = PlayerProperties.Position;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._intro_index = state.GetEntIndexByName("IntroCredits1");
                Debug.WriteLine("intro index is " + this._intro_index);
            }

            if (this.IsLastMap)
            {
                this._trig_index = state.GetEntIndexByName("AlyxWake1");
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && this._intro_index != -1)
            {
                var newintro = state.GetEntInfoByIndex(_intro_index);

                if (newintro.EntityPtr == IntPtr.Zero)
                {
                    _intro_index = -1;
                    Debug.WriteLine("deepdown start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && this._trig_index != -1)
            {
                var newtrig = state.GetEntInfoByIndex(_trig_index);

                if (newtrig.EntityPtr == IntPtr.Zero)
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
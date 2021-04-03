using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DeepDown : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        private bool _onceFlag;

        private int _introIndex;

        private float _splitTime;

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
                this._introIndex = state.GetEntIndexByName("IntroCredits1");
                Debug.WriteLine("intro index is " + this._introIndex);
            }

            if (this.IsLastMap)
            {
                _splitTime = state.FindOutputFireTime("Titles_music1", 7);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (state.CurrentMap.ToLower() == this.FirstMap && this._introIndex != -1)
            {
                var newIntro = state.GetEntInfoByIndex(_introIndex);

                if (newIntro.EntityPtr == IntPtr.Zero)
                {
                    _introIndex = -1;
                    _onceFlag = true;
                    Debug.WriteLine("deepdown start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (state.CurrentMap.ToLower() == this.LastMap)
            {
                float splitTime = state.FindOutputFireTime("AlyxWakeUp1", 7);

                if (_splitTime == 0f && splitTime != 0f)
                {
                    Debug.WriteLine("deepdown end");
                    _onceFlag = true;
                    _splitTime = splitTime;
                    return GameSupportResult.PlayerLostControl;
                }

                _splitTime = splitTime;
            }
            return GameSupportResult.DoNothing;
        }
    }
}

using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class LostCoast : GameSupport
    {
        // how to match with demos:
        // start: when the input to kill the blackout entity's parent is fired
        // ending: when the final output is fired by the trigger_once

        private bool _onceFlag = false;
        private float _splitTime;
        private float _splitTime2;

        public LostCoast()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "hdrtest"; //beta%
            this.LastMap = "d2_lostcoast";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                _splitTime = state.FindOutputFireTime("blackout", "Kill", "", 5);
                _splitTime2 = state.FindOutputFireTime("csystem_sound_start", "PlaySound", "", 5);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            float splitTime = state.FindOutputFireTime("blackout", "Kill", "", 8);

            if (_splitTime == 0f && splitTime != 0f)
            {
                Debug.WriteLine("lostcoast start");
                // no once flag because the end wont trigger otherwise
                _splitTime = splitTime;
                return GameSupportResult.PlayerGainedControl;
            }

            _splitTime = splitTime;

            float splitTime2 = state.FindOutputFireTime("csystem_sound_start", "PlaySound", "", 8);

            if (_splitTime2 == 0f && splitTime2 != 0f)
            {
                Debug.WriteLine("lostcoast end");
                _onceFlag = true;
                _splitTime2 = splitTime2;
                return GameSupportResult.PlayerLostControl;
            }

            _splitTime2 = splitTime2;
            return GameSupportResult.DoNothing;
        }
    }
}

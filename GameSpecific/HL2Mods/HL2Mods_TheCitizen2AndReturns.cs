using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_TheCitizen2AndReturns : GameSupport
    {
        // start: when the output to give the player the suit is fired AND a trigger in the level has not been hit
        // ending: 
            // the citizen 2: after the final fade
            // the citizen returns: on the first frame of the final fade

        private bool _onceFlag;

        private int _trigIndex;
        private float _splitTime;

        private MemoryWatcher<int> _fadeListSize;

        public HL2Mods_TheCitizen2AndReturns()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "sp_intro";
            this.LastMap = "sp_square";
        }

        public override void OnGameAttached(GameState state)
        {
            base.OnGameAttached(state);
            _fadeListSize = new MemoryWatcher<int>(state.GameOffsets.FadeListPtr + 0x10);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsFirstMap)
            {
                _splitTime = 0f;
                _trigIndex = state.GetEntIndexByPos(-1973f, -4511f, -1901.5f);
                Debug.WriteLine("target trigger found at " + _trigIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("commander", "Command", "give item_suit", 4);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                IntPtr trigPtr = state.GetEntInfoByIndex(_trigIndex).EntityPtr;
                if (trigPtr != IntPtr.Zero && state.CompareToInternalTimer(_splitTime, 0f, true))
                {
                    _onceFlag = true;
                    _splitTime = 0f;
                    Debug.WriteLine("the citizen 2 start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                float splitTime = state.FindFadeEndTime(-2560f);
                if (state.CompareToInternalTimer(splitTime))
                {
                    _onceFlag = true;
                    this.EndOffsetTicks = -1;
                    Debug.WriteLine("the citizen 2 end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            else if (state.CurrentMap.ToLower() == "sp_waterplant2")
            {
                _fadeListSize.Update(state.GameProcess);

                float splitTime = state.FindFadeEndTime(-127.5f);

                if (splitTime != 0f && _fadeListSize.Old == 0 && _fadeListSize.Current == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("the citizen returns end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

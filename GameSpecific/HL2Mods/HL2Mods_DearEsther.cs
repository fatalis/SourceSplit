using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DearEsther : GameSupport
    {
        // start: on first map
        // ending: when the final trigger is hit

        private bool _onceFlag;

        private int _trigIndex;

        public HL2Mods_DearEsther()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "donnelley";
            this.LastMap = "Paul";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsLastMap)
            {
                _trigIndex = state.GetEntIndexByName("triggerEndSequence");
                Debug.WriteLine("trigger index is " + _trigIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                var newTrig = state.GetEntInfoByIndex(_trigIndex);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    _onceFlag = true;
                    Debug.WriteLine("dearesther end");
                    this.EndOffsetTicks = 7;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

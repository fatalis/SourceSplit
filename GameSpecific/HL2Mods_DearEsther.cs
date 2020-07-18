using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DearEsther : GameSupport
    {
        // start: on first map
        // ending: when the final trigger is hit

        private bool _onceFlag;
        private static bool _resetFlag;

        private Vector3f _startPos = new Vector3f(3836f, 5620f, 350.395477f);

        private int _trigIndex;

        public HL2Mods_DearEsther()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "donnelley";
            this.LastMap = "Paul";
            this.RequiredProperties = PlayerProperties.Position;
        }

        public override void OnTimerReset(bool resetflagto)
        {
            _resetFlag = resetflagto;
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

            if (this.IsFirstMap && state.PlayerPosition.DistanceXY(_startPos) <= 0.05f && !_resetFlag)
            {
                _resetFlag = true;
                _onceFlag = true;
                Debug.WriteLine("dearesther start");
                return GameSupportResult.PlayerGainedControl;
            }

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

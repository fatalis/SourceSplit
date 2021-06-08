using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_ExperimentalFuel : GameSupport
    {
        // start: when block brush is killed
        // end: when a dustmote entity is killed by the switch

        private bool _onceFlag;
        private static bool _resetFlag;

        private int _blockBrushIndex;
        private int _dustmoteIndex;

        public HL2Mods_ExperimentalFuel()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "bmg1_experimental_fuel";
        }

        public override void OnTimerReset(bool resetFlagTo)
        {
            _resetFlag = resetFlagTo;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                _blockBrushIndex = state.GetEntIndexByName("dontrunaway");
                _dustmoteIndex = state.GetEntIndexByName("kokedepth");
            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                var newMote = state.GetEntInfoByIndex(_dustmoteIndex);
                var newBrush = state.GetEntInfoByIndex(_blockBrushIndex);

                if (state.PlayerPosition.DistanceXY(new Vector3f(7784.5f, 7284f, -15107f)) >= 2
                    && state.PrevPlayerPosition.DistanceXY(new Vector3f(7784.5f, 7284f, -15107f)) < 2
                    && newBrush.EntityPtr == IntPtr.Zero && !_resetFlag)
                {
                    Debug.WriteLine("exp fuel start");
                    _resetFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }

                if (newMote.EntityPtr == IntPtr.Zero)
                {
                    _onceFlag = true;
                    _dustmoteIndex = -1;
                    Debug.WriteLine("exp fuel end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

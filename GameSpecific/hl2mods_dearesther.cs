using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_dearesther : GameSupport
    {
        // start: on first map
        // ending: when the final trigger is hit

        private bool _onceFlag;
        private static bool resetflag;

        private Vector3f startpos = new Vector3f(3836f, 5620f, 350.395477f);

        private int trig_index;

        public hl2mods_dearesther()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "donnelley";
            this.LastMap = "Paul";
            this.RequiredProperties = PlayerProperties.Position;
        }

        public static void _resetflag()
        {
            resetflag = false;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsLastMap)
            {
                trig_index = state.GetEntIndexByName("triggerEndSequence");
                Debug.WriteLine("trigger index is " + trig_index);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && state.PlayerPosition.DistanceXY(startpos) <= 0.05f && resetflag == false)
            {
                resetflag = true;
                _onceFlag = true;
                Debug.WriteLine("dearesther start");
                return GameSupportResult.PlayerGainedControl;
            }

            if (this.IsLastMap)
            {
                var newtrig = state.GetEntInfoByIndex(trig_index);

                if (newtrig.EntityPtr == IntPtr.Zero)
                {
                    _onceFlag = true;
                    Debug.WriteLine("dearesther end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
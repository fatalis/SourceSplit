using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_watchingpaintdry : GameSupport
    {
        // start (all categories): on chapter select (only activates on timer reset)
        // ending (ice): 0.225 seconds (15 ticks) after button moves after it is pressed
        // ending (ee): 0.2 seconds (~ 14 ticks) after player is frozen by credits camera

        private bool _onceFlag;

        private Vector3f _crashbutton_newpos = new Vector3f(142.5f, 62f, 251f);
        private Vector3f _crashbutton_pos;
        private IntPtr _crashbutton_index;

        private Vector3f _start_pos = new Vector3f(192f, -24f, 1.96845f);

        public static int wpd_counter;

        public hl2mods_watchingpaintdry()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "wpd_st";
            this.FirstMap2 = "watchingpaintdry"; // the mod has 2 versions and for some reason the modder decided to start the 2nd with a completely different set of map names
            this.LastMap = "wpd_uni";
        }


        public static void workaround()
        {
            wpd_counter = 0;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
            {
                return GameSupportResult.DoNothing;
            }

            if (this.IsFirstMap || this.IsFirstMap2)
            {
                this._crashbutton_index = state.GetEntityByName("bonzibutton");
                state.GameProcess.ReadValue(_crashbutton_index + state.GameOffsets.BaseEntityAbsOriginOffset, out _crashbutton_pos);
                //Debug.WriteLine("_crashbutton_index pos is " + _crashbutton_pos);

                if (wpd_counter == 0 && state.PlayerPosition.Distance(_start_pos)<=0.001)
                {
                    wpd_counter += 1;
                    return GameSupportResult.PlayerGainedControl;
                }

                if (_crashbutton_pos.BitEquals(_crashbutton_newpos))
                {
                    Debug.WriteLine("wpd ice end");
                    _onceFlag = true;
                    this.EndOffsetTicks = 15;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            else if (this.IsLastMap)
            {
                if (state.PlayerFlags.HasFlag(FL.FROZEN))
                {
                    Debug.WriteLine("wpd ee end");
                    _onceFlag = true;
                    this.EndOffsetTicks = 134;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
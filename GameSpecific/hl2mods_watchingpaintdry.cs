using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_WatchingPaintDry : GameSupport
    {
        // start (all categories): on chapter select (only activates on timer reset)
        // ending (ice): 0.225 seconds (15 ticks) after button moves after it is pressed
        // ending (ee): 0.2 seconds (~ 14 ticks) after player is frozen by credits camera

        private bool _onceFlag;

        private Vector3f _crashbutton_Newpos = new Vector3f(142.5f, 62f, 251f);
        private Vector3f _crashbutton_Pos;
        private IntPtr _crashbutton_Index;

        private Vector3f _start_Pos = new Vector3f(192f, -24f, 1.96845f);

        public static bool _resetFlag = false;

        public HL2Mods_WatchingPaintDry()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "wpd_st";
            this.FirstMap2 = "watchingpaintdry"; // the mod has 2 versions and for some reason the modder decided to start the 2nd with a completely different set of map names
            this.LastMap = "wpd_uni";
            this.RequiredProperties = PlayerProperties.Flags | PlayerProperties.Position;
        }

        public override void OnTimerReset(bool resetflagto)
        {
            _resetFlag = resetflagto;
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
                this._crashbutton_Index = state.GetEntityByName("bonzibutton");
                state.GameProcess.ReadValue(_crashbutton_Index + state.GameOffsets.BaseEntityAbsOriginOffset, out _crashbutton_Pos);
                //Debug.WriteLine("_crashbutton_Index pos is " + _crashbutton_Pos);

                if (!_resetFlag && state.PlayerPosition.Distance(_start_Pos)<=0.001)
                {
                    _resetFlag = true;
                    Debug.WriteLine("wpd start");
                    return GameSupportResult.PlayerGainedControl;
                }

                if (_crashbutton_Pos.BitEquals(_crashbutton_Newpos))
                {
                    Debug.WriteLine("wpd ice end");
                    _onceFlag = true;
                    this.EndOffsetTicks = 15;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            else if (this.IsLastMap && state.PlayerFlags.HasFlag(FL.FROZEN))
            {
                    Debug.WriteLine("wpd ee end");
                    _onceFlag = true;
                    this.EndOffsetTicks = 134;
                    return GameSupportResult.PlayerLostControl;
            }
            return GameSupportResult.DoNothing;
        }
    }
}
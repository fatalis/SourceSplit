using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_WatchingPaintDry : GameSupport
    {
        // start (all categories): on chapter select
        // ending (ice): when the buttom moves
        // ending (ee): 0.2 seconds (~ 14 ticks) after player is frozen by credits camera

        private bool _onceFlag;

        private MemoryWatcher<Vector3f> _crashButtonPos;

        public HL2Mods_WatchingPaintDry()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.StartOnFirstMapLoad = true;
            this.FirstMap = "wpd_st";
            this.FirstMap2 = "watchingpaintdry"; // the mod has 2 versions and for some reason the modder decided to start the 2nd with a completely different set of map names
            this.LastMap = "wpd_uni";
            this.RequiredProperties = PlayerProperties.Flags | PlayerProperties.Position;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsFirstMap || IsFirstMap2)
            {
                this._crashButtonPos = new MemoryWatcher<Vector3f>(state.GetEntityByName("bonzibutton") + state.GameOffsets.BaseEntityAbsOriginOffset);
            }
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
                _crashButtonPos.Update(state.GameProcess);

                if (_crashButtonPos.Current.X > _crashButtonPos.Old.X && _crashButtonPos.Old.X != 0)
                {
                    Debug.WriteLine("wpd ice end");
                    _onceFlag = true;
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

using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_WatchingPaintDry : GameSupport
    {
        // start (all categories): on chapter select
        // ending (ice): when the buttom moves
        // ending (ee): when color correction entity is disabled
        // ending (cd): when the disconnect output is processed

        private bool _onceFlag;
        private float _splitTime;

        // todo: maybe sigscan this?
        private const int _baseColorCorrectEnabledOffset = 0x355;

        private MemoryWatcher<Vector3f> _crashButtonPos;
        private MemoryWatcher<byte> _colorCorrectEnabled;

        public HL2Mods_WatchingPaintDry()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "wpd_st";
            this.FirstMap2 = "watchingpaintdry"; // the mod has 2 versions and for some reason the modder decided to start the 2nd with a completely different set of map names
            this.LastMap = "wpd_uni";
            this.StartOnFirstLoadMaps.AddRange(new string[] { this.FirstMap, this.FirstMap2 });
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsFirstMap || IsFirstMap2)
            {
                this._crashButtonPos = new MemoryWatcher<Vector3f>(state.GetEntityByName("bonzibutton") + state.GameOffsets.BaseEntityAbsOriginOffset);
            }
            else if (IsLastMap)
            {
                this._colorCorrectEnabled = new MemoryWatcher<byte>(state.GetEntityByName("Color_Correction") + _baseColorCorrectEnabledOffset);
            }
            _onceFlag = false;
            _splitTime = 0f;
        }

        public override void OnGenericUpdate(GameState state)
        {
            if (state.CurrentMap.ToLower() == "wpd_tp" || state.CurrentMap.ToLower() == "hallway")
            {
                float splitTime = state.FindOutputFireTime("commands", "Command", "disconnect", 5);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;

                if (state.CompareToInternalTimer(_splitTime, 0f, false, true) && !_onceFlag)
                {
                    Debug.WriteLine("wdp ce ending");
                    _splitTime = 0f;
                    _onceFlag = true;
                    this.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

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
            else if (this.IsLastMap)
            {
                _colorCorrectEnabled.Update(state.GameProcess);

                if (_colorCorrectEnabled.Current == 0 && _colorCorrectEnabled.Old == 1)
                {
                    Debug.WriteLine("wpd ee end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

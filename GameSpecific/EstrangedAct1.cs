using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class EstrangedAct1 : GameSupport
    {
        // start: when the title screen card stops being active
        // ending: when final trigger_once is hit, breaking the floor

        private bool _onceFlag;

        private const int _interactiveScreenActiveFlag = 0x338;

        private MemoryWatcher<byte> _titleCardActive;
        private int _trig2Index;

        public EstrangedAct1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "sp01thebeginning";
            this.LastMap = "sp10thewarehouse";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
                this._titleCardActive = new MemoryWatcher<byte>(state.GetEntityByName("gillnetter_titlecard") + _interactiveScreenActiveFlag);
            else if (this.IsLastMap)
            {
                this._trig2Index = state.GetEntIndexByPos(5240f, -7800f, -206f);
                Debug.WriteLine("trig2 index is " + this._trig2Index);
            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                _titleCardActive.Update(state.GameProcess);
                if (_titleCardActive.Old == 1 && _titleCardActive.Current == 0)
                {
                    Debug.WriteLine("estranged2 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && this._trig2Index != -1)
            {
                var newTrig2 = state.GetEntInfoByIndex(_trig2Index);

                if (newTrig2.EntityPtr == IntPtr.Zero)
                {
                    _trig2Index = -1;
                    Debug.WriteLine("estranged1 end");
                    _onceFlag = true;
                    this.EndOffsetTicks = (int)Math.Ceiling(0.1f / state.IntervalPerTick);
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

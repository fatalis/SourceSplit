using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class te120 : GameSupport
    {
        // start: when player view entity changes
        // ending: when player has godmode AND all striders are killed (monitored by their look triggers being killed)

        private const int ENT_INDEX_PLAYER = 1;
        private bool _onceFlag;

        private int _cam_index;

        private IntPtr stride1;
        private IntPtr stride2;
        private IntPtr stride3;

        public te120()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "chapter_1";
            this.LastMap = "chapter_4";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._cam_index = state.GetEntIndexByName("blackout_viewcontrol");
                Debug.WriteLine("blackout_viewcontrol index is " + this._cam_index);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
            {
                return GameSupportResult.DoNothing;
            }

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex == this._cam_index
                    && state.PlayerViewEntityIndex == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("te120 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap && state.PlayerFlags.HasFlag(FL.GODMODE))
            {
                this.stride1 = state.GetEntityByName("tl_crush_1");
                this.stride2 = state.GetEntityByName("tl_crush_2");
                this.stride3 = state.GetEntityByName("tl_crush_3");

                if (stride1 == IntPtr.Zero && stride2 == IntPtr.Zero && stride3 == IntPtr.Zero)
                {
                    _onceFlag = true;
                    Debug.WriteLine("te120 end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
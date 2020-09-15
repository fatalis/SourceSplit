using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class TE120 : GameSupport
    {
        // start: when player view entity changes
        // ending: when player has godmode AND all striders are killed (monitored by their look triggers being killed)

        private bool _onceFlag;

        private int _camIndex;

        private IntPtr _stride1Ptr;
        private IntPtr _stride2Ptr;
        private IntPtr _stride3Ptr;

        public TE120()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "chapter_1";
            this.LastMap = "chapter_4";
            this.RequiredProperties = PlayerProperties.ViewEntity | PlayerProperties.Flags;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._camIndex = state.GetEntIndexByName("blackout_viewcontrol");
                Debug.WriteLine("blackout_viewcontrol index is " + this._camIndex);
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
                if (state.PrevPlayerViewEntityIndex == this._camIndex
                    && state.PlayerViewEntityIndex == GameState.ENT_INDEX_PLAYER)
                {
                    Debug.WriteLine("te120 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            // TODO: improve this to not rely on entity pointers
            else if (this.IsLastMap && state.PlayerFlags.HasFlag(FL.GODMODE) && !state.PrevPlayerFlags.HasFlag(FL.GODMODE))
            {
                this._stride1Ptr = state.GetEntityByName("tl_crush_1");
                this._stride2Ptr = state.GetEntityByName("tl_crush_2");
                this._stride3Ptr = state.GetEntityByName("tl_crush_3");

                if (_stride1Ptr == IntPtr.Zero && _stride2Ptr == IntPtr.Zero && _stride3Ptr == IntPtr.Zero)
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

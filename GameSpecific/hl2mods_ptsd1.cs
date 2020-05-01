using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_ptsd1 : GameSupport
    {
        // how to match with demos:
        // start: after player view entity changes (requires debug config and checks to avoid other changes)
        // ending: when breen's banana hat (yes really) is killed

        private bool _onceFlag;

        private int _breen_index;
        public static bool resetflag;

        public hl2mods_ptsd1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ptsd_1";
            this.LastMap = "ptsd_final";
            this.RequiredProperties = PlayerProperties.Position;
        }

        //this is a check so that the game doesn't split every time the player view entity changes. 
        //normally this isn't needed but for this mod we need to do this
        //the game will only start if resetflag is 0, on every change this counter is increased so the game won't split on another change
        //this will be reset when the timer is reset

        public static void workaround() 
        {
            resetflag = false;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._breen_index = state.GetEntIndexByName("banana2");
                Debug.WriteLine("banana2 index is " + this._breen_index);
            }
        
             _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex != GameState.ENT_INDEX_PLAYER
                    && state.PlayerViewEntityIndex == GameState.ENT_INDEX_PLAYER && resetflag == false)
                {
                    Debug.WriteLine("ptsd start");
                    _onceFlag = true;
                    resetflag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && this._breen_index != -1)
            {
                var newblack = state.GetEntInfoByIndex(_breen_index);

                if (newblack.EntityPtr == IntPtr.Zero)
                {
                    _breen_index = -1;
                    Debug.WriteLine("ptsd end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
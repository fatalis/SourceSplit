using LiveSplit.ComponentUtil;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_toomanycrates : GameSupport
    {
        // start: on first map
        // ending: when the end text model's skin code is 10 and player view entity switches to the final camera

        private bool _onceFlag;
        private static bool resetflag;

        private int counter_index;
        private int cam_index;

        private int skinoffset = 872;

        public hl2mods_toomanycrates()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "cratastrophy";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public static void _resetflag()
        {
            resetflag = false;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsFirstMap)
            {
                counter_index = state.GetEntIndexByName("EndWords");
                cam_index = state.GetEntIndexByName("EndCamera");
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                var crate = state.GetEntInfoByIndex(counter_index);
                int d;

                state.GameProcess.ReadValue(crate.EntityPtr + skinoffset, out d);

                if (resetflag == false)
                {
                    resetflag = true;
                    Debug.WriteLine("toomanycrates start");
                    return GameSupportResult.PlayerGainedControl;
                }
                    
                if (d == 10 && state.PlayerViewEntityIndex == cam_index)
                {
                    _onceFlag = true;
                    Debug.WriteLine("toomanycrates start");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}
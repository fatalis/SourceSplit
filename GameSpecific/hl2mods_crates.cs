using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_toomanycrates : GameSupport
    {
        // start: on first map
        // ending: when the end text model's skin code is 10 and player view entity switches to the final camera

        private bool _onceFlag;
        private static bool _resetFlag;

        private int _counter_Index;
        private int _cam_Index;

        private Vector3f startpos = new Vector3f(-2587.32f, 0f, -3.32f);

        private const int skinoffset = 872;

        public hl2mods_toomanycrates()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "cratastrophy";
            this.RequiredProperties = PlayerProperties.ViewEntity | PlayerProperties.Position;
        }

        public override void OnTimerReset(bool resetflagto)
        {
            _resetFlag = resetflagto;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsFirstMap)
            {
                _counter_Index = state.GetEntIndexByName("EndWords");
                _cam_Index = state.GetEntIndexByName("EndCamera");
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                var crate = state.GetEntInfoByIndex(_counter_Index);
                int d;

                state.GameProcess.ReadValue(crate.EntityPtr + skinoffset, out d);

                if (!_resetFlag && state.PlayerEntInfo.EntityPtr != IntPtr.Zero && state.PlayerPosition.Distance(startpos) <= 0.05f)
                {
                  _resetFlag = true;
                  Debug.WriteLine("toomanycrates start");
                  return GameSupportResult.PlayerGainedControl;
                }
                    
                if (d == 10 && state.PlayerViewEntityIndex == _cam_Index)
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
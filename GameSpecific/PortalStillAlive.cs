using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalStillAlive : GameSupport
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on game disconnect

        private MemoryWatcher<Vector3f> _elevatorPos;
        private bool _onceFlag;

        public PortalStillAlive()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AutoStartType = AutoStart.ViewEntityChanged;
            this.FirstMap = "stillalive_1";
            this.LastMap = "stillalive_14";
            this.EndOffsetTicks = -1;
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap)
                _elevatorPos = new MemoryWatcher<Vector3f>(state.GetEntityByName("a10_a11_elevator_body") + state.GameOffsets.BaseEntityAbsOriginOffset);

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                _elevatorPos.Update(state.GameProcess);
                if (_elevatorPos.Current.Z < 3760)
                    return GameSupportResult.DoNothing;

                float splitTime = state.FindOutputFireTime("client_command", 20);
                if (state.CompareToInternalTimer(splitTime))
                {
                    _onceFlag = true;
                    Debug.WriteLine("portal still alive end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

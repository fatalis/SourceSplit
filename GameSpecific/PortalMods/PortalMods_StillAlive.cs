using LiveSplit.ComponentUtil;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_StillAlive : GameSupport
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on game disconnect

        private MemoryWatcher<Vector3f> _elevatorPos;
        private float _splitTime;
        private bool _onceFlag;

        public PortalMods_StillAlive()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "stillalive_1";
            this.LastMap = "stillalive_14";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnGenericUpdate(GameState state)
        {
            if (state.HostState == HostState.GameShutdown)
                this.OnUpdate(state);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap)
                _elevatorPos = new MemoryWatcher<Vector3f>(state.GetEntityByName("a10_a11_elevator_body") + state.GameOffsets.BaseEntityAbsOriginOffset);

            _splitTime = 0f;
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            float splitTime = 0f;
            if (this.IsLastMap)
            {
                _elevatorPos.Update(state.GameProcess);
                if (_elevatorPos.Current.Z >= 3760)
                    splitTime = state.FindOutputFireTime("client_command", 10);
            }
            else
                splitTime = state.FindOutputFireTime("command", "Command", "map ", 10, true, false, true);

            if (splitTime != 0f)
                _splitTime = splitTime;

            if (state.CompareToInternalTimer(_splitTime, 0f, false, true))
            {
                _splitTime = 0f;
                Debug.WriteLine("portal still alive " + (!this.IsLastMap ? "split" : "end"));
                _onceFlag = true;
                QueueOnNextSessionEnd = this.IsLastMap ? GameSupportResult.PlayerLostControl : GameSupportResult.ManualSplit;
            }

            return GameSupportResult.DoNothing;
        }

    }
}

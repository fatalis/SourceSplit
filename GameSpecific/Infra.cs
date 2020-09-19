using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Infra : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // endings: all on fades

        private bool _onceFlag = false;

        private MemoryWatcher<int> _fadeListSize;
        private MemoryWatcher<uint> _fadeListPtr;
        private MemoryWatcherList _fadeListWatcher = new MemoryWatcherList();

        public Infra()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.StartOnFirstMapLoad = true;
            this.FirstMap = "infra_c1_m1_office";
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe client = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "client.dll");
            Trace.Assert(client != null);

            var scanner = new SignatureScanner(state.GameProcess, client.BaseAddress, client.ModuleMemorySize);
            var fadelisttarget = new SigScanTarget(2, "8D 88 ?? ?? ?? ?? 8B 01 8B 40 ?? 8D 55 ??");

            IntPtr fadelistptr = scanner.Scan(fadelisttarget);

            if (fadelistptr != IntPtr.Zero)
                Debug.WriteLine("Fade list pointer found at 0x" + fadelistptr.ToString("X"));

            // for some reason making this IntPtr makes it pick up an extra byte...
            _fadeListPtr = new MemoryWatcher<uint>(state.GameProcess.ReadPointer(fadelistptr) + 0x4);
            _fadeListSize = new MemoryWatcher<int>(state.GameProcess.ReadPointer(fadelistptr) + 0x10);

            _fadeListWatcher = new MemoryWatcherList() { _fadeListSize, _fadeListPtr };
        }

        // env_fades don't hold any live fade information and instead they network over fade infos to the client which add it to a list
        // fade speed is pretty much the only thing we can use to differentiate one from others
        // this function will find a fade with a specified speed and then return the timestamp for when the fade ends
        public float FindFadeEndTime(GameState state, float speed)
        {
            float tmpSpeed;
            for (int i = 0; i < _fadeListSize.Current; i++)
            {
                tmpSpeed = state.GameProcess.ReadValue<float>(state.GameProcess.ReadPointer((IntPtr)_fadeListPtr.Current) + 0x4 * i);
                if (tmpSpeed != speed)
                    continue;
                else
                    return state.GameProcess.ReadValue<float>(state.GameProcess.ReadPointer((IntPtr)_fadeListPtr.Current) + 0x4 * i + 0x4);
            }
            return 0;
        }

        public GameSupportResult DefaultEnd(GameState state, float fadeSpeed, string ending)
        {
            float splitTime = FindFadeEndTime(state, fadeSpeed);
            // this is how the game actually knows when a fade has finished as well
            if (splitTime != 0f && Math.Abs(splitTime - state.RawTickCount * state.IntervalPerTick) <= 0.05f)
            {
                _onceFlag = true;
                Debug.WriteLine("infra " + ending + " ending");
                return GameSupportResult.PlayerLostControl;
            }
            else
                return GameSupportResult.DoNothing;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            _fadeListWatcher.UpdateAll(state.GameProcess);

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            switch (state.CurrentMap.ToLower())
            {
                default:
                    return GameSupportResult.DoNothing;

                case "infra_c5_m2b_sewer2":
                    {
                        return DefaultEnd(state, -2560f, "part 1");
                    }
                case "infra_c7_m5_powerstation":
                    {
                        // v2 and v3 have different start and end durations since v2 ends in a credits sequence 
                        var test = DefaultEnd(state, -85f, "part 2");
                        if (test == GameSupportResult.DoNothing)
                            return DefaultEnd(state, -2560f, "part 2");
                        else
                            return test;
                    }
                case "infra_c11_ending_1":
                case "infra_c11_ending_2":
                case "infra_c11_ending_3":
                    {
                        return DefaultEnd(state, -25.5f, "part 3");
                    }
            }
        }
    }
}

using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{      
    class HL2Mods_Logistique : GameSupport
    {
        // start: on first map
        // ending: when the first outro credits text appear on the screen

        private bool _onceFlag;

        private MemoryWatcher<float> _creditsYPos;
        private MemoryWatcher<int> _creditsCount;
        private MemoryWatcher<int> _yResolution;

        private MemoryWatcherList _watcher = new MemoryWatcherList();

        public HL2Mods_Logistique()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "Lg-1";
            this.LastMap = "Lg-4";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnGameAttached(GameState state)
        {
            base.OnGameAttached(state);

            ProcessModuleWow64Safe vguimatsurface = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "vguimatsurface.dll");
            Trace.Assert(vguimatsurface != null);

            // there are other cleaner pointers but vguimatsurface is most unlikely to change
            // i would've tried to find a sigscanned method but the pointer is extremely hard to get to
            _creditsYPos = new MemoryWatcher<float>(new DeepPointer(vguimatsurface.BaseAddress + 0x147120, 0xF00, 0x2C, 0x9D4 + 0x1BC, 0x200));
            _creditsCount = new MemoryWatcher<int>(new DeepPointer(vguimatsurface.BaseAddress + 0x147120, 0xF00, 0x2C, 0x9D4 + 0x1c8));
            _yResolution = new MemoryWatcher<int>(vguimatsurface.BaseAddress + 0x136C28);

            _watcher = new MemoryWatcherList { _creditsYPos, _creditsCount, _yResolution };
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                _watcher.UpdateAll(state.GameProcess);

                if (_creditsCount.Current >= 1 && _creditsYPos.Changed 
                    && _yResolution.Current / _creditsYPos.Current >= 1.0175f
                    && _yResolution.Current / _creditsYPos.Old < 1.0175f)
                {
                    _onceFlag = true;
                    Debug.WriteLine("logistique end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

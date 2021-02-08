using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HLS : GameSupport
    {
        // start: on first map
        // ending: when nihi's hp drops down to 1 or lower

        private bool _onceFlag;

        private int _baseEntityHealthOffset = -1;

        private MemoryWatcher<int> _nihiHP;

        public HLS()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "c1a0";
            this.LastMap = "c4a3";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }
        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsLastMap)
            {
                _nihiHP = new MemoryWatcher<int>(state.GetEntityByName("nihilanth") + _baseEntityHealthOffset);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                _nihiHP.Update(state.GameProcess);
                if (_nihiHP.Current <= 1 && _nihiHP.Old > 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("hls end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

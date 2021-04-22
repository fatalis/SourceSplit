using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Tinje : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // end: when final guard is killed

        private MemoryWatcher<int> _tinjeGuardHP;
        private int _baseEntityHealthOffset = -1;
        private bool _onceFlag;

        public HL2Mods_Tinje()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "tinje";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnGenericUpdate(GameState state) { }

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
            _onceFlag = false;

            if (IsFirstMap)
                _tinjeGuardHP = new MemoryWatcher<int>(state.GetEntityByName("end") + _baseEntityHealthOffset);

        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                _tinjeGuardHP.Update(state.GameProcess);
                if (_tinjeGuardHP.Current <= 0 && _tinjeGuardHP.Old > 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("tinje end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

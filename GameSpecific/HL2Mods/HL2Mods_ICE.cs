using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_ICE : GameSupport
    {
        // start: on first map
        // ending: when the gunship's hp drops hits or drops below 0hp

        private bool _onceFlag;
        private MemoryWatcher<int> _gunshipHP;

        private int _baseEntityHealthOffset = -1;

        public HL2Mods_ICE()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ice_02";
            this.LastMap = "ice_32";
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

            if (IsLastMap && _baseEntityHealthOffset != 0x0)
            {
                _gunshipHP = new MemoryWatcher<int>(state.GetEntityByName("helicopter_1") + _baseEntityHealthOffset);
            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                _gunshipHP.Update(state.GameProcess);
                if (_gunshipHP.Current <= 0 && _gunshipHP.Old > 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("ice end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

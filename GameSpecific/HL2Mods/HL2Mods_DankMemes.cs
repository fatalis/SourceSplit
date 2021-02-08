using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DankMemes : GameSupport
    {
        // start: on first map
        // ending: when "John Cena" (final antlion king _bossPtr) hp is <= 0 

        private bool _onceFlag;

        private int _baseEntityHealthOffset = -1;

        private MemoryWatcher<int> _bossHP;

        public HL2Mods_DankMemes()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "Your_house";
            this.LastMap = "Dank_Boss";
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

            if (this.IsLastMap)
                _bossHP = new MemoryWatcher<int>(state.GetEntityByName("John_Cena") + _baseEntityHealthOffset);

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                _bossHP.Update(state.GameProcess);

                if (_bossHP.Current <= 0 && _bossHP.Old > 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("dank memes end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

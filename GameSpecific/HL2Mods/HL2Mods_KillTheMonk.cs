using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_KillTheMonk : GameSupport
    {
        // start: when the player's view entity index changes back to 1
        // ending: when the monk's hp drop to 0

        private bool _onceFlag;
        private int _baseEntityHealthOffset = -1;

        private int _camIndex;
        private MemoryWatcher<int> _monkHP;

        public HL2Mods_KillTheMonk()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ktm_c01_01";
            this.LastMap = "ktm_c03_02";
            this.RequiredProperties = PlayerProperties.ViewEntity;
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

            if (IsFirstMap)
            {
                _camIndex = state.GetEntIndexByName("blackout_cam");
                Debug.WriteLine("start cam index is " + _camIndex);
            }
            else if (IsLastMap && _baseEntityHealthOffset != -1)
            {
                _monkHP = new MemoryWatcher<int>(state.GetEntityByName("Monk") + _baseEntityHealthOffset);
            }

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerViewEntityIndex == _camIndex && state.PlayerViewEntityIndex == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("kill the monk start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (IsLastMap)
            {
                _monkHP.Update(state.GameProcess);

                if (_monkHP.Current <= 0 && _monkHP.Old > 0)
                {
                    Debug.WriteLine("kill the monk end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

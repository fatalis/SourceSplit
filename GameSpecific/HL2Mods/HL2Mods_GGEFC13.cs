using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_GGEFC13 : GameSupport
    {
        // start: on input to teleport the player
        // ending: when the helicopter's hp drops to 0 or lower

        private bool _onceFlag;
        private float _splitTime;
        private int _baseEntityHealthOffset = -1;
        private MemoryWatcher<int> _heliHP;

        public HL2Mods_GGEFC13()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ge_city01";
            this.LastMap = "ge_final";
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

            if (this.IsFirstMap)
                _splitTime = state.FindOutputFireTime("teleport_trigger");
            else if (this.IsLastMap)
                _heliHP = new MemoryWatcher<int>(state.GetEntityByName("helicopter") + _baseEntityHealthOffset);

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("teleport_trigger", 5);
                _splitTime = (splitTime == 0f) ? _splitTime : splitTime;
                if (state.CompareToInternalTimer(_splitTime, 0f, true))
                {
                    Debug.WriteLine("ggefc13 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                _heliHP.Update(state.GameProcess);

                if (_heliHP.Current <= 0 && _heliHP.Old > 0)
                {
                    Debug.WriteLine("ggefc13 end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

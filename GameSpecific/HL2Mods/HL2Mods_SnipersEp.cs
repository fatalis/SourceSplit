using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_SnipersEp : GameSupport
    {
        //start: when player moves (excluding an on-the-spot jump)
        //end: when "gordon" is killed (hp is <= 0)

        private bool _onceFlag;
        private int _baseEntityHealthOffset = -1;
        public static bool _resetFlag;

        private MemoryWatcher<int> _freemanHP;
        Vector3f _startPos = new Vector3f(9928f, 12472f, -180f);

        public HL2Mods_SnipersEp()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "bestmod2013";
            this.RequiredProperties = PlayerProperties.Position;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        public override void OnTimerReset(bool resetFlagTo)
        {
            _resetFlag = resetFlagTo;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;

            if (this.IsFirstMap)
                _freemanHP = new MemoryWatcher<int>(state.GetEntityByName("bar") + _baseEntityHealthOffset);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                if (state.PrevPlayerPosition.BitEqualsXY(_startPos) && !state.PlayerPosition.BitEqualsXY(_startPos) && !_resetFlag)
                {
                    _resetFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }

                _freemanHP.Update(state.GameProcess);
                if (_freemanHP.Current <= 0 && _freemanHP.Old > 0)
                {
                    Debug.WriteLine("snipersep end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

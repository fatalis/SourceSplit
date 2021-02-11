using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Freakman1 : GameSupport
    {
        // start: when the start trigger is hit
        // ending: when kleiner's hp is <= 0

        private bool _onceFlag;

        private int _baseEntityHealthOffset = -1;

        private int _trigIndex;
        private MemoryWatcher<int> _kleinerHP;

        public HL2Mods_Freakman1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "gordon1";
            this.LastMap = "endbattle";
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
            {
                _trigIndex = state.GetEntIndexByPos(-1472f, -608f, 544f);
                Debug.WriteLine("start trigger index is " + _trigIndex);
            }
            _onceFlag = false;

            if (this.IsLastMap)
            {
                _kleinerHP = new MemoryWatcher<int>(state.GetEntInfoByIndex(state.GetEntIndexByPos(0f, 0f, 1888f, 1f)).EntityPtr + _baseEntityHealthOffset);
            }
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _trigIndex != -1)
            {
                var newTrig = state.GetEntInfoByIndex(_trigIndex);

                if (newTrig.EntityPtr == IntPtr.Zero)
                {
                    _trigIndex = -1;
                    _onceFlag = true;
                    Debug.WriteLine("freakman1 start");
                    this.StartOffsetTicks = -7;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                _kleinerHP.Update(state.GameProcess);
                if (_kleinerHP.Current <= 0 && _kleinerHP.Old > 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("freakman1 end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

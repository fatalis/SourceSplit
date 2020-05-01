using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_freakman1 : GameSupport
    {
        // start: when intro text entity is killed
        // ending: when the trigger for alyx to do her wake up animation is hit

        private bool _onceFlag;

        private int _baseEntityHealthOffset = -1;

        private int trig_index;
        private int _kleiner_index;

        public hl2mods_freakman1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "gordon1";
            this.LastMap = "endbattle";
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

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                trig_index = state.GetEntIndexByPos(-1472f, -608f, 544f);
            }
            _onceFlag = false;

            if (this.IsLastMap)
            {
                _kleiner_index = state.GetEntIndexByPos(0f, 0f, 1888f, 1f);
                Debug.WriteLine("kleiner index is " + _kleiner_index);
            }
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && trig_index != -1)
            {
                var newtrig = state.GetEntInfoByIndex(trig_index);
                
                if (newtrig.EntityPtr == IntPtr.Zero)
                {
                    trig_index = -1;
                    _onceFlag = true;
                    Debug.WriteLine("freakman1 start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap && _kleiner_index != -1)
            {
                var kleiner = state.GetEntInfoByIndex(_kleiner_index);
                int hp;
                state.GameProcess.ReadValue(kleiner.EntityPtr + _baseEntityHealthOffset, out hp);

                if (hp <= 0)
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
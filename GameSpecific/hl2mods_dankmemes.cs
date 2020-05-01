using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_dankmemes : GameSupport
    {
        // start: on first map
        // ending: when "John Cena" (final antlion king boss) hp is <= 0 

        private bool _onceFlag;
        private static bool _resetflag;

        private int _baseEntityHealthOffset = -1;

        IntPtr boss;

        public hl2mods_dankmemes()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "Your_house";
            this.LastMap = "Dank_Boss";
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

        public static void resetflag()
        {
            _resetflag = false;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap)
            {
                boss = state.GetEntityByName("John_Cena");
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _resetflag == false)
            {
                _resetflag = true;
                Debug.WriteLine("dank memes start");
                return GameSupportResult.PlayerGainedControl;
            }

            else if (this.IsLastMap)
            {
                int hp;
                state.GameProcess.ReadValue(boss + _baseEntityHealthOffset, out hp);

                if (hp <= 0)
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
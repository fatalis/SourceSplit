using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DankMemes : GameSupport
    {
        // start: on first map
        // ending: when "John Cena" (final antlion king _boss_Ptr) hp is <= 0 

        private bool _onceFlag;
        private static bool _resetFlag;

        private int _baseEntityHealthOffset = -1;

        private int _cam_Index;
        IntPtr _boss_Ptr;

        public HL2Mods_DankMemes()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "Your_house";
            this.LastMap = "Dank_Boss";
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

        public override void OnTimerReset(bool resetflagto)
        {
            _resetFlag = resetflagto;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                _cam_Index = state.GetEntIndexByName("black_cam");
            }

            if (this.IsLastMap)
            {
                _boss_Ptr = state.GetEntityByName("John_Cena");
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && !_resetFlag && state.PlayerViewEntityIndex == _cam_Index)
            {
                _resetFlag = true;
                Debug.WriteLine("dank memes start");
                return GameSupportResult.PlayerGainedControl;
            }

            else if (this.IsLastMap)
            {
                int hp;
                state.GameProcess.ReadValue(_boss_Ptr + _baseEntityHealthOffset, out hp);

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

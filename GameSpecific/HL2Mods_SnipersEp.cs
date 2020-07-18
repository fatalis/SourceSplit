using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_SnipersEp : GameSupport
    {
        //start: when player moves (excluding an on-the-spot jump)
        //end: when "gordon" is killed (hp is <= 0)

        private bool _onceFlag;
        private int _baseEntityHealthOffset = -1;
        public static bool _resetFlag;

        IntPtr _freemanPtr;
        Vector3f _startPos = new Vector3f(9928f, 12472f, -180f);

        public HL2Mods_SnipersEp()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "bestmod2013";
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

            _onceFlag = false;

            if (this.IsFirstMap)
            {
                _freemanPtr = state.GetEntityByName("bar");
                Debug.WriteLine("freeman ptr is 0x" + _freemanPtr.ToString("X"));
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
            {
                return GameSupportResult.DoNothing;
            }

            if (this.IsFirstMap)
            {
                if (!state.PlayerPosition.BitEqualsXY(_startPos) && !_resetFlag)
                {
                    _resetFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }

                int hp;
                state.GameProcess.ReadValue(_freemanPtr + _baseEntityHealthOffset, out hp);
                if (hp <= 0)
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

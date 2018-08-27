using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class BMSRetail : GameSupport
    {
        // how to match with demos:
        // start: first tick when your position is at 113 -1225 582 (cl_showpos 1)
        // ending: first tick when view is locked to 0 0 1 (cl_showpos 1)

        private bool _onceFlag;
        private Vector3f _startPos = new Vector3f(113f, -1225f, 582f);
        private int _baseCombatCharacaterActiveWeaponOffset = -1;
        private int _baseEntityHealthOffset = -1;
        private int _prevActiveWeapon;
        private int _prevViewEntity;

        public BMSRetail()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "bm_c1a0a";
            this.LastMap = "bm_c3a2i";
            this.RequiredProperties = PlayerProperties.Position | PlayerProperties.ViewEntity;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_hActiveWeapon", state.GameProcess, scanner, out _baseCombatCharacaterActiveWeaponOffset))
                Debug.WriteLine("CBaseCombatCharacater::m_hActiveWeapon offset = 0x" + _baseCombatCharacaterActiveWeaponOffset.ToString("X"));
            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            _onceFlag = false;

            if (this.IsLastMap && _baseCombatCharacaterActiveWeaponOffset != -1 && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _baseCombatCharacaterActiveWeaponOffset, out _prevActiveWeapon);
        }
        
        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            // map starts
            if (this.IsFirstMap)
            {
                if (state.PlayerPosition.DistanceXY(_startPos) < 1.0f)
                {
                    Debug.WriteLine("black mesa start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && _baseCombatCharacaterActiveWeaponOffset != -1 && state.PlayerEntInfo.EntityPtr != IntPtr.Zero
                && _baseEntityHealthOffset != -1)
            {
                // "OnTrigger" "stripper,StripWeaponsAndSuit,,0,-1"
                // "OnTrigger" "locked_in,Enable,,0,-1"

                int activeWeapon;
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _baseCombatCharacaterActiveWeaponOffset, out activeWeapon);
                int viewEntity;
                viewEntity = state.PlayerViewEntityIndex;
                if (activeWeapon == -1 && _prevActiveWeapon == -1
                    && viewEntity != _prevViewEntity)
                {
                    int health;
                    state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset, out health);

                    if (health > 0)
                    {
                        Debug.WriteLine("black mesa end");
                        _onceFlag = true;
                        return GameSupportResult.PlayerLostControl;
                    }
                }
                _prevActiveWeapon = activeWeapon;
                _prevViewEntity = viewEntity;
            }
            return GameSupportResult.DoNothing;
        }
    }
}

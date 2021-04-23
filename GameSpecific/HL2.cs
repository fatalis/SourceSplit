using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2 : GameSupport
    {
        // how to match with demos:
        // start: first tick when your position is at -9419 -2483 22 (cl_showpos 1)
        // ending: first tick when screen flashes white

        private bool _onceFlag;

        private Vector3f _startPos = new Vector3f(-9419f, -2483f, 22f);
        private int _baseCombatCharacaterActiveWeaponOffset = -1;
        private int _baseEntityHealthOffset = -1;
        private int _prevActiveWeapon;

        private HL2Mods_TheLostCity _lostCity = new HL2Mods_TheLostCity();
        private HL2Mods_Tinje _tinje = new HL2Mods_Tinje();
        private HL2Mods_ExperimentalFuel _experimentalFuel = new HL2Mods_ExperimentalFuel();

        public HL2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "d1_trainstation_01";
            this.LastMap = "d3_breen_01";
            this.RequiredProperties = PlayerProperties.Position;

            NonStandaloneMods = new List<GameSupport>(new GameSupport[] { _lostCity, _tinje, _experimentalFuel });
            foreach (GameSupport mod in NonStandaloneMods)
                this.StartOnFirstLoadMaps.AddRange(mod.StartOnFirstLoadMaps);
        }

        public override void OnGameAttached(GameState state)
        {
            base.OnGameAttached(state);

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

            if (this.IsFirstMap) 
            {
                // "OnTrigger" "point_teleport_destination,Teleport,,0.1,-1"

                // first tick player is moveable and on the train
                if (state.PlayerPosition.DistanceXY(_startPos) <= 1.0)
                {
                    Debug.WriteLine("hl2 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && _baseCombatCharacaterActiveWeaponOffset != -1 && state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _baseEntityHealthOffset != -1)
            {
                // "OnTrigger2" "weaponstrip_end_game,Strip,,0,-1"
                // "OnTrigger2" "fade_blast_1,Fade,,0,-1"

                int activeWeapon;
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _baseCombatCharacaterActiveWeaponOffset, out activeWeapon);

                if (activeWeapon == -1 && _prevActiveWeapon != -1
                    && state.PlayerPosition.Distance(new Vector3f(-2449.5f, -1380.2f, -446.0f)) > 256f) // ignore the initial strip that happens at around 2.19 seconds
                {
                    int health;
                    state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset, out health);

                    if (health > 0)
                    {
                        Debug.WriteLine("hl2 end");
                        _onceFlag = true;
                        return GameSupportResult.PlayerLostControl;
                    }
                }

                _prevActiveWeapon = activeWeapon;
            }
            else
            {
                return base.OnUpdate(state);
            }

            return GameSupportResult.DoNothing;
        }
    }
}

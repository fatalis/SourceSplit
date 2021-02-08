using System;
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
        
        // experimental fuel:
        // start: when block brush is killed
        // end: when a dustmote entity is killed by the switch

        // tinje:
        // start: on map load
        // end: when final guard is killed

        private bool _onceFlag;

        private static bool _expfuelStartFlag;

        private Vector3f _startPos = new Vector3f(-9419f, -2483f, 22f);
        private int _baseCombatCharacaterActiveWeaponOffset = -1;
        private int _baseEntityHealthOffset = -1;
        private int _prevActiveWeapon;

        private int _efBlockBrushIndex;
        private int _efDustmoteIndex;

        private MemoryWatcher<int> _tinjeGuardHP;

        public HL2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "d1_trainstation_01";
            this.StartOnFirstLoadMaps.Add("tinje");
            this.LastMap = "d3_breen_01";
            this.RequiredProperties = PlayerProperties.Position;
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

        public override void OnTimerReset(bool resetFlagTo)
        {
            _expfuelStartFlag = resetFlagTo;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            _onceFlag = false;

            if (this.IsLastMap && _baseCombatCharacaterActiveWeaponOffset != -1 && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _baseCombatCharacaterActiveWeaponOffset, out _prevActiveWeapon);


            // some of these mods are just maps that you put into half-life 2
            switch (state.CurrentMap.ToLower())
            {
                case "tinje":
                    _tinjeGuardHP = new MemoryWatcher<int>(state.GetEntityByName("end") + _baseEntityHealthOffset);
                    break;

                case "bmg1_experimental_fuel":
                    _efBlockBrushIndex = state.GetEntIndexByName("dontrunaway");
                    _efDustmoteIndex = state.GetEntIndexByName("kokedepth");
                    break;
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            switch (state.CurrentMap.ToLower())
            {
                default:
                    {
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
                        break;
                    }

                case "bmg1_experimental_fuel":
                    {
                        var newMote = state.GetEntInfoByIndex(_efDustmoteIndex);
                        var newBrush = state.GetEntInfoByIndex(_efBlockBrushIndex);

                        if (state.PlayerPosition.DistanceXY(new Vector3f(7784.5f, 7284f, -15107f)) >= 2 && state.PrevPlayerPosition.DistanceXY(new Vector3f(7784.5f, 7284f, -15107f)) < 2
                            && newBrush.EntityPtr == IntPtr.Zero && !_expfuelStartFlag)
                        {
                            Debug.WriteLine("exp fuel start");
                            _expfuelStartFlag = true;
                            return GameSupportResult.PlayerGainedControl;
                        }

                        if (newMote.EntityPtr == IntPtr.Zero)
                        {
                            _onceFlag = true;
                            _efDustmoteIndex = -1;
                            Debug.WriteLine("exp fuel end");
                            return GameSupportResult.PlayerLostControl;
                        }
                        break;
                    }

                case "tinje":
                    {
                        _tinjeGuardHP.Update(state.GameProcess);
                        if (_tinjeGuardHP.Current <= 0 && _tinjeGuardHP.Old > 0)
                        {
                            _onceFlag = true;
                            Debug.WriteLine("tinje end");
                            return GameSupportResult.PlayerLostControl;
                        }
                        break;
                    }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

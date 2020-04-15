using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_ptsd1 : GameSupport
    {
        // how to match with demos:
        // start: after player view entity changes (requires debug config and checks to avoid other changes)
        // ending: when breen's banana hat (yes really) is killed

        private bool _onceFlag;

        private int _baseCombatCharacaterActiveWeaponOffset = -1;
        private int _baseEntityHealthOffset = -1;
        private int _breen_index;
        public static int startcount;

        public hl2mods_ptsd1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ptsd_1";
            this.LastMap = "ptsd_final";
            this.RequiredProperties = PlayerProperties.Position;
        }

        //this is a check so that the game doesn't split every time the player view entity changes. 
        //normally this isn't needed but for this mod we need to do this
        //the game will only start if startcount is 0, on every change this counter is increased so the game won't split on another change
        //this will be reset when the timer is reset

        public static void workaround() 
        {
            startcount = 0;
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

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._breen_index = state.GetEntIndexByName("banana2");
                Debug.WriteLine("banana2 index is " + this._breen_index);
            }
        
                _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

                if (this.IsFirstMap)
            {
                if (state.PlayerViewEntityIndex != state.PrevPlayerViewEntityIndex && startcount == 0)
                {
                    Debug.WriteLine("ptsd start");
                    _onceFlag = true;
                    startcount = startcount + 1;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && this._breen_index != -1)
            {
                var newblack = state.GetEntInfoByIndex(_breen_index);

                if (newblack.EntityPtr == IntPtr.Zero)
                {
                    _breen_index = -1;
                    Debug.WriteLine("ptsd end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
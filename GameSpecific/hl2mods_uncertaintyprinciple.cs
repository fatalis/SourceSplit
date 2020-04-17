using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_uncertaintyprinciple : GameSupport
    {
        // start: 2 seconds after the camera's parent entity gets within 8 units of the final path_track
        // ending: when player is frozen by the camera entity

        private const int ENT_INDEX_PLAYER = 1;
        private bool _onceFlag;

        private int _baseCombatCharacaterActiveWeaponOffset = -1;
        private int _baseEntityHealthOffset = -1;

        private IntPtr _track_index;
        private IntPtr _cam_index;
        private CEntInfoV2 player;

        Vector3f trackpos;
        Vector3f campos;

        public hl2mods_uncertaintyprinciple()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "up_retreat_a";
            this.LastMap = "up_night";
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

            if (this.IsFirstMap)
            {
                this._track_index = state.GetEntityByName("start_cam_corner2");
                state.GameProcess.ReadValue(_track_index + state.GameOffsets.BaseEntityAbsOriginOffset, out trackpos);
                Debug.WriteLine("trackpos pos is " + trackpos);
            }

            if (this.IsLastMap)
            { 
                this.player = state.GetEntInfoByIndex(ENT_INDEX_PLAYER);

            }

            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
            {
                return GameSupportResult.DoNothing;
            }

            if (this.IsFirstMap)
            {
                this._cam_index = state.GetEntityByName("camera1_train");
                state.GameProcess.ReadValue(_cam_index + state.GameOffsets.BaseEntityAbsOriginOffset, out campos);
                Debug.WriteLine("campos is " + campos);

                if (campos.DistanceXY(trackpos) <= 8)
                {
                    Debug.WriteLine("up start");
                    this.StartOffsetTicks = 134;
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap)
            {
                FL playerflags;
                state.GameProcess.ReadValue(player.EntityPtr + state.GameOffsets.BaseEntityFlagsOffset, out playerflags);

                if (playerflags.HasFlag(FL.FROZEN))
                {
                    Debug.WriteLine("up end");
                    _onceFlag = true;
                   return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
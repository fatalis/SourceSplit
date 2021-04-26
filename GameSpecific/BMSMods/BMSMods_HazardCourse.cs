using LiveSplit.ComponentUtil;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class BMSMods_HazardCourse : GameSupport
    {
        // how to match with demos:
        // start: when the tram door is opening
        // end: when the flash sprites disappears

        private bool _onceFlag = false;

        // offsets and binary sizes
        private int _baseEffectsFlagsOffset = -1;

        // hc mod start & end
        private Vector3f _hcStartDoorTargPos = new Vector3f(4152.7f, -2853.1f, 105f);
        private MemoryWatcher<Vector3f> _hcStartDoorPos;
        private MemoryWatcher<uint> _hcEndSpriteFlags;

        public BMSMods_HazardCourse()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "hc_t0a0";
            this.LastMap = "hc_t0a3";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");
            Trace.Assert(server != null);

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_fEffects", state.GameProcess, scanner, out _baseEffectsFlagsOffset))
                Debug.WriteLine("CBaseEntity::m_fEffects offset = 0x" + _baseEffectsFlagsOffset.ToString("X"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            _onceFlag = false;

            if (IsFirstMap)
            {
                _hcStartDoorPos = new MemoryWatcher<Vector3f>(state.GetEntityByName("tram_door_door_out") + state.GameOffsets.BaseEntityAbsOriginOffset);
                _hcStartDoorPos.Update(state.GameProcess);
            }
            else if (IsLastMap)
            {
                _hcEndSpriteFlags = new MemoryWatcher<uint>(state.GetEntityByName("spr_camera_flash") + _baseEffectsFlagsOffset);
                _hcEndSpriteFlags.Update(state.GameProcess);
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                _hcStartDoorPos.Update(state.GameProcess);

                if (_hcStartDoorPos.Old.Distance(_hcStartDoorTargPos) > 0.05f
                    && _hcStartDoorPos.Current.Distance(_hcStartDoorTargPos) <= 0.05f)
                {
                    _onceFlag = true;
                    Debug.WriteLine("bms hc mod start");
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (IsLastMap)
            {
                _hcEndSpriteFlags.Update(state.GameProcess);

                if (state.TickCount >= 10 && (_hcEndSpriteFlags.Old & 0x20) == 0 &&
                    (_hcEndSpriteFlags.Current & 0x20) != 0)
                {
                    _onceFlag = true;
                    Debug.WriteLine("bms hc mod end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
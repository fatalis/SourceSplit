using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Portal : GameSupport
    {
        // how to match this timing with demos:
        // start: 
            // portal: crosshair appear
            // portal tfv map pack: on first map

        private bool _onceFlag;
        private int _laggedMovementOffset = -1;
        private const int VAULT_SAVE_TICK = 4261;
        private int _gladosIndex;

        public Portal()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AutoStartType = AutoStart.ViewEntityChanged;
            this.FirstMap = "testchmb_a_00";
            this.LastMap = "escape_02";        
            this.RequiredProperties = PlayerProperties.Position | PlayerProperties.ViewEntity;
            this.AdditionalGameSupport.Add(new PortalMods_TheFlashVersion());
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _laggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _laggedMovementOffset.ToString("X"));
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._gladosIndex = state.GetEntIndexByName("glados_body");
                Debug.WriteLine("Glados index is " + this._gladosIndex);
            }
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            switch (state.CurrentMap.ToLower())
            {
                case "testchmb_a_00":
                    {
                        // vault save starts at tick 4261, but update interval may miss it so be a little lenient
                        if (state.TickBase >= VAULT_SAVE_TICK && state.TickBase <= VAULT_SAVE_TICK + 4)
                        {
                            _onceFlag = true;
                            int ticksSinceVaultSaveTick = state.TickBase - VAULT_SAVE_TICK; // account for missing ticks if update interval missed it
                            this.StartOffsetTicks = -3534 - ticksSinceVaultSaveTick; // 53.01 seconds
                            return GameSupportResult.PlayerGainedControl;
                        }
                        this.StartOffsetTicks = 1;
                        return base.OnUpdate(state);
                    }
                case "escape_02":
                    {
                        if (this._gladosIndex != -1)
                        {
                            var newglados = state.GetEntInfoByIndex(_gladosIndex);

                            if (newglados.EntityPtr == IntPtr.Zero)
                            {
                                Debug.WriteLine("robot lady boom detected");
                                _onceFlag = true;
                                this.EndOffsetTicks = -1;
                                return GameSupportResult.PlayerLostControl;
                            }
                        }
                        break;
                    }
            }
            return GameSupportResult.DoNothing;
        }
    }
}
using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.Extensions;

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
        private float _splitTime = 0;
        private int _gladosIndex;
        private Vector3f _vaultStartPos = new Vector3f(-544f, -368.776001f, 160.031250f);
        private CustomCommand _enableNewStart = new CustomCommand("newstart", description: "Start the timer upon portal open");
        private CustomCommandHandler _ccHandler;

        public Portal()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("testchmb_a_00");
            this.AddLastMap("escape_02");        
            this.RequiredProperties = PlayerProperties.Position | PlayerProperties.ViewEntity;
            this.AdditionalGameSupport.Add(new PortalMods_TheFlashVersion());
            _ccHandler = new CustomCommandHandler(_enableNewStart);
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _laggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _laggedMovementOffset.ToString("X"));

            _ccHandler.Init(state);
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (IsFirstMap)
                _splitTime = state.FindOutputFireTime("relay_portal_cancel_room1", 50);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._gladosIndex = state.GetEntIndexByName("glados_body");
                Debug.WriteLine("Glados index is " + this._gladosIndex);
            }
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            _ccHandler.Update(state);

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                bool isInside = state.PlayerPosition.InsideBox(-636, -452, -412, -228, 383, 158);

                if (_enableNewStart.Enabled)
                {
                    float splitTime = state.FindOutputFireTime("relay_portal_cancel_room1", 50);
                    try
                    {
                        if (_splitTime != 0 && splitTime == 0)
                        {
                            this.StartOffsetTicks = -3801;
                            Debug.WriteLine("portal start");
                            _onceFlag = true;
                            return isInside ? GameSupportResult.PlayerGainedControl : GameSupportResult.DoNothing;
                        }
                    }
                    finally { _splitTime = splitTime; }
                }
                else
                {
                    // vault save starts at tick 4261, but update interval may miss it so be a little lenient
                    // player must be somewhere within the vault as well due to new vault skip
                    if (isInside &&
                        state.TickBase >= VAULT_SAVE_TICK
                        && state.TickBase <= VAULT_SAVE_TICK + 4)
                    {
                        _onceFlag = true;
                        int ticksSinceVaultSaveTick = state.TickBase - VAULT_SAVE_TICK; // account for missing ticks if update interval missed it
                        this.StartOffsetTicks = -3534 - ticksSinceVaultSaveTick; // 53.01 seconds
                        return GameSupportResult.PlayerGainedControl;
                    }
                }


                if (isInside && state.PrevPlayerViewEntityIndex != state.PlayerViewEntityIndex
                    && state.PlayerViewEntityIndex == 1)
                {
                    this.StartOffsetTicks = 1;
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (IsLastMap)
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
            }

            return GameSupportResult.DoNothing;
        }
    }
}
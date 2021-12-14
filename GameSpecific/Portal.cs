using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.Extensions;
using LiveSplit.SourceSplit.Utils;

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
        private int _baseEntityHealthOffset = -1;
        private const int VAULT_SAVE_TICK = 4261;
        private float _splitTime = 0;
        private float _elevSplitTime = 0;
        private MemoryWatcher<int> _playerHP;
        private int _gladosIndex;

        private CustomCommand _newStart = new CustomCommand("newstart", "0", "Start the timer upon portal open");
        private CustomCommand _elevSplit = new CustomCommand("elevsplit", "0", "Split when the elevator starts moving (limited)");
        private CustomCommand _deathSplit = new CustomCommand("death%", "0", "Death% ending");
        private CustomCommandHandler _ccHandler;

        public Portal()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("testchmb_a_00");
            this.AddLastMap("escape_02");        
            this.RequiredProperties = PlayerProperties.Position | PlayerProperties.ViewEntity;
            this.AdditionalGameSupport.Add(new PortalMods_TheFlashVersion());
            _ccHandler = new CustomCommandHandler(_newStart, _elevSplit, _deathSplit);
        }

        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            if (GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state.GameProcess, scanner, out _laggedMovementOffset))
                Debug.WriteLine("CBasePlayer::m_flLaggedMovementValue offset = 0x" + _laggedMovementOffset.ToString("X"));

            if (GameMemory.GetBaseEntityMemberOffset("m_iHealth", state.GameProcess, scanner, out _baseEntityHealthOffset))
                Debug.WriteLine("CBaseEntity::m_iHealth offset = 0x" + _baseEntityHealthOffset.ToString("X"));

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

            if (_elevSplit.BValue)
            {
                _elevSplitTime = state.FindOutputFireTime("*elev_start", 15);
                Debug.WriteLine("Elevator split time is " + _elevSplitTime);
            }

            _playerHP = new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _baseEntityHealthOffset);

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            _ccHandler.Update(state);
            _playerHP.Update(state.GameProcess);

            if (_elevSplit.BValue)
            {
                float splitTime = 0;

                TryMany findSplitTime = new TryMany(
                    () => splitTime == 0,
                    () => splitTime = state.FindOutputFireTime("*elevator_start", 15),
                    () => splitTime = state.FindOutputFireTime("*elevator_door_model_close", 15));

                findSplitTime.Begin();

                try
                {
                    if (splitTime == 0 && _splitTime != 0)
                    {
                        Debug.WriteLine("Elevator began moving!");
                        return GameSupportResult.ManualSplit;
                    }
                }
                finally { _splitTime = splitTime; }
            }

            if (_deathSplit.BValue)
            {
                if (_playerHP.Old > 0 && _playerHP.Current <= 0)
                {
                    Debug.WriteLine("Death% end");
                    return GameSupportResult.ManualSplit;
                }
            }

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (IsFirstMap)
            {
                bool isInside = state.PlayerPosition.InsideBox(-636, -452, -412, -228, 383, 158);

                if (_newStart.BValue)
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
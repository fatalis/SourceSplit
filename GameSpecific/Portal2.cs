using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Portal2 : GameSupport
    {
        // how to match this timing with demos:
        // start: crosshair appear
        // ending: crosshair disappear

        private IntPtr _endDetectEntity;
        private float _lastEntCheckTime;
        private bool _prevCanShoot;
        private bool _onceFlag;

        // update if it breaks in the future
        private const int CPropVehicleChoreoGenericPlayerCanShootOffset = 0x880;

        public Portal2()
        {
            this.AutoStartType = AutoStart.ViewEntityChanged;
            this.AddFirstMap("sp_a1_intro1");
            this.AddLastMap("sp_a4_finale4");
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            _endDetectEntity = IntPtr.Zero;
            _lastEntCheckTime = 0;
            _prevCanShoot = true;
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (this.IsFirstMap)
                return base.OnUpdate(state);
            else if (!this.IsLastMap || _onceFlag)
                return GameSupportResult.DoNothing;

            // GetEntityByName is rather expensive so don't check every tick
            // there's a period of about 10-15 seconds from when it's created until it's used, so a 5 second
            // interval should be good enough
            if (_endDetectEntity == IntPtr.Zero && state.TickTime - _lastEntCheckTime > 5.0f)
            {
                Debug.WriteLine("checking for ending_vehicle ent");
                _endDetectEntity = state.GetEntityByName("ending_vehicle");
                _lastEntCheckTime = state.TickTime;

                if (_endDetectEntity != IntPtr.Zero)
                {
                    Debug.WriteLine("_endDetectEntity = 0x" + _endDetectEntity.ToString("X"));
                    state.GameProcess.ReadValue(_endDetectEntity + CPropVehicleChoreoGenericPlayerCanShootOffset, out _prevCanShoot);
                }
            }

            // "OnTrigger" "ending_vehicle SetCanShoot 0 0 -1"
            if (_endDetectEntity != IntPtr.Zero)
            {
                bool canShoot;
                state.GameProcess.ReadValue(_endDetectEntity + CPropVehicleChoreoGenericPlayerCanShootOffset, out canShoot);

                if (!canShoot && _prevCanShoot)
                {
                    Debug.WriteLine("portal 2 ending detection");
                    _onceFlag = true;
                    _endDetectEntity = IntPtr.Zero;
                    return GameSupportResult.PlayerLostControl;
                }

                _prevCanShoot = canShoot;
            }

            return GameSupportResult.DoNothing;
        }
    }
}

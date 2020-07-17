using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_UncertaintyPrinciple : GameSupport
    {
        // start: 2 seconds after the camera's parent entity gets within 8 units of the final path_track
        // ending: when player is frozen by the camera entity

        private bool _onceFlag;

        private IntPtr _track_Index;
        private IntPtr _cam_Index;

        Vector3f _trackPos;
        Vector3f _camPos;

        public HL2Mods_UncertaintyPrinciple()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "up_retreat_a";
            this.LastMap = "up_night";
            this.RequiredProperties = PlayerProperties.Flags;
        }


        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                this._track_Index = state.GetEntityByName("start_cam_corner2");
                this._cam_Index = state.GetEntityByName("camera1_train");
                state.GameProcess.ReadValue(_track_Index + state.GameOffsets.BaseEntityAbsOriginOffset, out _trackPos);
                Debug.WriteLine("_trackPos pos is " + _trackPos);
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
                state.GameProcess.ReadValue(_cam_Index + state.GameOffsets.BaseEntityAbsOriginOffset, out _camPos);
                Debug.WriteLine("_camPos is " + _camPos);

                if (_camPos.DistanceXY(_trackPos) <= 8)
                {
                    Debug.WriteLine("up start");
                    this.StartOffsetTicks = 134;
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }

            else if (this.IsLastMap)
            {
                if (state.PlayerFlags.HasFlag(FL.FROZEN))
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

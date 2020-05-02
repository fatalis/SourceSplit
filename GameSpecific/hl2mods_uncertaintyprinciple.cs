using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class hl2mods_uncertaintyprinciple : GameSupport
    {
        // start: 2 seconds after the camera's parent entity gets within 8 units of the final path_track
        // ending: when player is frozen by the camera entity

        private bool _onceFlag;

        private IntPtr _track_index;
        private IntPtr _cam_index;

        Vector3f trackpos;
        Vector3f campos;

        public hl2mods_uncertaintyprinciple()
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
                this._track_index = state.GetEntityByName("start_cam_corner2");
                this._cam_index = state.GetEntityByName("camera1_train");
                state.GameProcess.ReadValue(_track_index + state.GameOffsets.BaseEntityAbsOriginOffset, out trackpos);
                Debug.WriteLine("trackpos pos is " + trackpos);
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
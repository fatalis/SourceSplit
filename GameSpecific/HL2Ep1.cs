using System;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Ep1 : GameSupport
    {
        // how to match with demos:
        // start: crosshair appear
        // ending: the first tick where your position changes on while on train (cl_showpos 1)

        private bool _onceFlag;
        private IntPtr _endDetectEntity;
        private IntPtr _startDetectEntity;

        // initial position of the train before it starts moving
        private Vector3f _trainStartPos = new Vector3f(11957.6f, 8368.25f, -731.75f);

        public HL2Ep1()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ep1_citadel_00";
            this.LastMap = "ep1_c17_06";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            _onceFlag = false;
            _startDetectEntity = IntPtr.Zero;
            _endDetectEntity = IntPtr.Zero;

            if (this.IsFirstMap)
                _startDetectEntity = state.GetEntityByName("ghostanim_DogIntro");
            else if (this.IsLastMap)
                _endDetectEntity = state.GetEntityByName("outro_train_1");
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap && _startDetectEntity != IntPtr.Zero)
            {
                // "PlayerOff" "ghostanim_DogIntro,Kill,,0,-1"

                FL flags;
                state.GameProcess.ReadValue(_startDetectEntity + state.GameOffsets.BaseEntityFlagsOffset, out flags);
                if (flags.HasFlag(FL.KILLME))
                {
                    Debug.WriteLine("ep1 start");
                    _onceFlag = true;
                    _startDetectEntity = IntPtr.Zero;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap && _endDetectEntity != IntPtr.Zero)
            {
                // "OnTrigger" "razortrain3,StartForward,,0,-1"
                // "OnTrigger" "outro_train_1,SetParent,razortrain3,0,-1"

                Vector3f trainPos;
                if (!state.GameProcess.ReadValue(_endDetectEntity + state.GameOffsets.BaseEntityAbsOriginOffset, out trainPos))
                    return GameSupportResult.DoNothing;

                // if the train started moving, stop timing
                if (!trainPos.BitEquals(_trainStartPos))
                {
                    Debug.WriteLine("ep1 end");
                    _onceFlag = true;
                    _endDetectEntity = IntPtr.Zero;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_SchoolAdventures : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // ending: when the output to enable the final teleport trigger is fired

        private bool _onceFlag;

        public HL2Mods_SchoolAdventures()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "sa_01";
            this.LastMap = "sa_04";
            this.StartOnFirstMapLoad = true;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                float splitTime = state.FindOutputFireTime("final_teleport1",3);
                if (state.CompareToInternalTimer(splitTime))
                {
                    _onceFlag = true;
                    Debug.WriteLine("school_adventures end");
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

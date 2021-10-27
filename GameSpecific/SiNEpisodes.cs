using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class SiNEpisodes : GameSupport
    {
        // how to match with demos:
        // start: on first map

        public SiNEpisodes()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "se1_docks01";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }
    }
}

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_BackwardsMod : GameSupport
    {
        // start: on first map
        public HL2Mods_BackwardsMod()
        {
            this.StartOnFirstMapLoad = true;
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "backward_d3_breen_01";
            this.StartOnFirstMapLoad = true;
        }

    }
}

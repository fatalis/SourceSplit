using LiveSplit.ComponentUtil;
using System.Collections.Generic;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class Synergy : GameSupport
    {
        // how to match with demos:
        // start: on map load
        // xen start: when view entity changes back to the player's
        // ending: first tick nihilanth's health is zero
        // earthbound ending: when view entity changes to the ending camera's

        private bool _onceFlag;

        private CustomCommand _autosplitIL = new CustomCommand("ilstart");
        private List<string> _startMaps = new List<string>() { "d1_trainstation_01", "ep1_citadel_00", "ep2_outland_01" };
        private CustomCommandHandler _cmdHandler;

        public Synergy()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            _cmdHandler = new CustomCommandHandler( new CustomCommand[] { _autosplitIL });
        }

        public override void OnGameAttached(GameState state)
        {
            _cmdHandler.Init(state);
        }

        public override void OnGenericUpdate(GameState state)
        {
            if (_autosplitIL.Enabled)
            {
                if (!StartOnFirstLoadMaps.Contains(state.CurrentMap))
                {
                    StartOnFirstLoadMaps.Clear();
                    StartOnFirstLoadMaps.Add(state.CurrentMap);
                }
            }
            else
                StartOnFirstLoadMaps.Clear();
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            _cmdHandler.Update(state);

            if (_onceFlag)
                return GameSupportResult.DoNothing;

            return GameSupportResult.DoNothing;
        }
    }
}
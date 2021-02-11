using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_DaBaby : GameSupport
    {
        // start: on first map
        // ending: when the player's view entity index changes to ending camera's

        private bool _onceFlag;

        private int _endingCamIndex;

        public HL2Mods_DaBaby()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "dababy_hallway_ai";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                _endingCamIndex = state.GetEntIndexByName("final_viewcontrol");
                Debug.WriteLine("found end cam index at " + _endingCamIndex);
            }

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (_endingCamIndex != -1)
            {
                if (state.PlayerViewEntityIndex == _endingCamIndex && state.PrevPlayerViewEntityIndex == 1)
                {
                    Debug.WriteLine("da baby end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }
            return GameSupportResult.DoNothing;
        }
    }
}

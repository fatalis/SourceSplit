using LiveSplit.ComponentUtil;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_TooManyCrates : GameSupport
    {
        // start: on first map
        // ending: when the end text model's skin code is 10 and player view entity switches to the final camera

        private bool _onceFlag;

        private MemoryWatcher<int> _counterSkin;
        private int _camIndex;

        private const int _baseSkinOffset = 872;

        public HL2Mods_TooManyCrates()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "cratastrophy";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsFirstMap)
            {
                _counterSkin = new MemoryWatcher<int>(state.GetEntityByName("EndWords") + _baseSkinOffset);
                _camIndex = state.GetEntIndexByName("EndCamera");
                Debug.WriteLine("found end cam index at " + _camIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                _counterSkin.Update(state.GameProcess);
                if (_counterSkin.Current == 10 && state.PlayerViewEntityIndex == _camIndex && state.PrevPlayerViewEntityIndex == 1)
                {
                    _onceFlag = true;
                    Debug.WriteLine("toomanycrates end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_MImp : GameSupport
    {
        // how to match with demos:
        // start: when cave_giveitems_equipper is called
        // ending: when player's view entity changes

        private bool _onceFlag;

        private int _camIndex;
        private float _splitTime;

        public HL2Mods_MImp()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "mimp1";
            this.LastMap = "mimp3";
            this.RequiredProperties = PlayerProperties.ViewEntity;
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
            {
                _splitTime = state.FindOutputFireTime("cave_giveitems_equipper", 5);
            }
            else if (this.IsLastMap)
            {
                this._camIndex = state.GetEntIndexByName("outro.camera");
                Debug.WriteLine("_camIndex index is " + this._camIndex);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                float newSplitTime = state.FindOutputFireTime("cave_giveitems_equipper", 5);
                if (_splitTime != 0f && newSplitTime == 0f)
                {
                    _onceFlag = true;
                    Debug.WriteLine("mimp start");
                    return GameSupportResult.PlayerGainedControl;
                }
                _splitTime = newSplitTime;
            }
            else if (this.IsLastMap && _camIndex != -1)
            {
                if (state.PlayerViewEntityIndex == _camIndex && state.PrevPlayerViewEntityIndex != _camIndex)
                {
                    Debug.WriteLine("mimp end");
                    _onceFlag = true;
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

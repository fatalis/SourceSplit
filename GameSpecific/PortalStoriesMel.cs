namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalStoriesMel : GameSupport
    {
        private Vector3f _endPos = new Vector3f(48f, 2000f, 288f);
        private bool _onceFlag;

        public PortalStoriesMel()
        {
            this.AutoStartType = AutoStart.Unfrozen;
            this.RequiredProperties |= PlayerProperties.Position;
            this.FirstMap = "sp_a1_tramride";
            this.LastMap = "sp_a4_finale";
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (this.IsFirstMap)
                return base.OnUpdate(state);
            else if (!this.IsLastMap || _onceFlag)
                return GameSupportResult.DoNothing;

            // "OnPressed" "end_teleport Teleport 0 -1"
            if (state.PlayerPosition.DistanceXY(_endPos) <= 1.0)
            {
                _onceFlag = true;
                return GameSupportResult.PlayerLostControl;
            }

            return GameSupportResult.DoNothing;
        }
    }
}

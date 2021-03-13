using System;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class PortalMods_PCBORRR : GameSupport
    {
        // how to match this timing with demos:
        // start: on first map load
        // ending: on glados' body entity being killed

        private bool _onceFlag;

        private int _gladosIndex;

        public PortalMods_PCBORRR()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "testchmb_a_00";
            this.LastMap = "escape_02_d_180";
            this.StartOnFirstLoadMaps.Add(this.FirstMap);
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsLastMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                this._gladosIndex = state.GetEntIndexByName("glados_body");
                Debug.WriteLine("Glados index is " + this._gladosIndex);
            }

            _onceFlag = false;
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                if (this._gladosIndex != -1)
                {
                    var newglados = state.GetEntInfoByIndex(_gladosIndex);

                    if (newglados.EntityPtr == IntPtr.Zero)
                    {
                        Debug.WriteLine("robot lady boom detected");
                        _onceFlag = true;
                        this.EndOffsetTicks = -1;
                        return GameSupportResult.PlayerLostControl;
                    }
                }
            }

            return GameSupportResult.DoNothing;
        }

    }
}

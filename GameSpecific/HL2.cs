using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2 : GameSupport
    {
        // how to match with demos:
        // start: first tick when your position is at -9419 -2483 22 (cl_showpos 1)
        // ending: first tick when screen flashes white

        private bool _onceFlag;
        private float _splitTime;

        private Vector3f _startPos = new Vector3f(-9419f, -2483f, 22f);

        private HL2Mods_TheLostCity _lostCity = new HL2Mods_TheLostCity();
        private HL2Mods_Tinje _tinje = new HL2Mods_Tinje();
        private HL2Mods_ExperimentalFuel _experimentalFuel = new HL2Mods_ExperimentalFuel();

        public HL2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "d1_trainstation_01";
            this.LastMap = "d3_breen_01";
            this.RequiredProperties = PlayerProperties.Position;

            AdditionaGameSupport = new List<GameSupport>(new GameSupport[] { _lostCity, _tinje, _experimentalFuel });
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            _onceFlag = false;

            if (this.IsLastMap)
                _splitTime = state.FindOutputFireTime("sprite_end_final_explosion_1", "ShowSprite", "", 20);
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap) 
            {
                // "OnTrigger" "point_teleport_destination,Teleport,,0.1,-1"

                // first tick player is moveable and on the train
                if (state.PlayerPosition.DistanceXY(_startPos) <= 1.0)
                {
                    Debug.WriteLine("hl2 start");
                    _onceFlag = true;
                    return GameSupportResult.PlayerGainedControl;
                }
            }
            else if (this.IsLastMap)
            {
                // "OnTrigger2" "weaponstrip_end_game,Strip,,0,-1"
                // "OnTrigger2" "fade_blast_1,Fade,,0,-1"
                float splitTime = state.FindOutputFireTime("sprite_end_final_explosion_1", "ShowSprite", "", 20);
                try
                {
                    if (splitTime > 0 && _splitTime == 0)
                    {
                        Debug.WriteLine("hl2 end");
                        _onceFlag = true;
                        return GameSupportResult.PlayerLostControl;
                    }
                }
                finally
                {
                    _splitTime = splitTime;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

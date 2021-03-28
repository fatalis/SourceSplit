using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Mods_Ptsd2 : GameSupport
    {
        // how to match with demos:
        // start: after output to unfreeze player is fired
        // ending: when the byte for if a video is playing turns from 0 to 1

        private bool _onceFlag;

        private float _splitTime;
        private MemoryWatcher<byte> _videoPlaying;

        public HL2Mods_Ptsd2()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.FirstMap = "ptsd_2_p1";
            this.LastMap = "ptsd_2_final_day";
        }

        public override void OnGameAttached(GameState state)
        {
            var bink = state.GameProcess.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "video_bink.dll");
            Trace.Assert(bink != null);

            var binkScanner = new SignatureScanner(state.GameProcess, bink.BaseAddress, bink.ModuleMemorySize);

            SigScanTarget target = new SigScanTarget(11, "C7 05 ?? ?? ?? ?? ?? ?? ?? ?? B9 ?? ?? ?? ??");
            target.OnFound = (proc, scanner, ptr) => {
                ptr = proc.ReadPointer(ptr) + 0xC;
                Debug.WriteLine("bink is video playing pointer found at 0x" + ptr.ToString("X"));
                return ptr;
            };

            _videoPlaying = new MemoryWatcher<byte>(binkScanner.Scan(target));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);

            if (this.IsFirstMap)
                _splitTime = state.FindOutputFireTime("scream", "PlaySound", "", 5);

            _onceFlag = false;
        }

        public override void OnGenericUpdate(GameState state)
        {
            if (this.IsLastMap)
            {
                _videoPlaying.Update(state.GameProcess);

                if (_videoPlaying.Old == 0 && _videoPlaying.Current == 1)
                {
                    Debug.WriteLine("ptsd end");
                    _onceFlag = true;
                    this.QueueOnNextSessionEnd = GameSupportResult.PlayerLostControl;
                }
            }
        }

        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsFirstMap)
            {
                float splitTime = state.FindOutputFireTime("scream", "PlaySound", "", 5);

                if (splitTime == 0f && _splitTime != 0f)
                {
                    Debug.WriteLine("ptsd start");
                    _onceFlag = true;
                    _splitTime = splitTime;
                    return GameSupportResult.PlayerGainedControl;
                }

                _splitTime = splitTime;
            }
            return GameSupportResult.DoNothing;
        }
    }
}

using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LiveSplit.SourceSplit.Extensions;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HLS : GameSupport
    {
        // start: on first map
        // ending: when nihi's hp drops down to 1 or lower

        private bool _onceFlag;
        private int _nihiDeadOffset = -1;
        private MemoryWatcher<bool> _nihiDead;

        public HLS()
        {
            this.GameTimingMethod = GameTimingMethod.EngineTicksWithPauses;
            this.AddFirstMap("c1a0");
            this.AddLastMap("c4a3");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMap);
        }
        public override void OnGameAttached(GameState state)
        {
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);

            this.EndOffsetTicks = 0;


            IntPtr getStringPtr(string str)
            {
                return scanner.Scan(new SigScanTarget(0, str.ConvertToHex() + " 00"));
            }

            IntPtr getPtrRef(IntPtr ptr, SignatureScanner scanner, params string[] prefixes)
            {
                if (ptr == IntPtr.Zero)
                    return ptr;
                string ptrStr = ptr.GetByteString();
                SigScanTarget target = new SigScanTarget();
                prefixes.ToList().ForEach(x => target.AddSignature(0, x + " " + ptrStr));
                return scanner.Scan(target);
            }

            IntPtr ptr;
            SigScanTarget target;

            if ((ptr = getPtrRef(getStringPtr("n_max"), scanner, "68")) == IntPtr.Zero)
                return;

            bool found = false;
            target = new SigScanTarget(1, "68");
            target.OnFound = (f_proc, f_scanner, f_ptr) =>
            {
                IntPtr ptr = f_proc.ReadPointer(f_ptr);
                found = !(ptr.ToInt32() < scanner.Address.ToInt32()
                    || ptr.ToInt32() > scanner.Address.ToInt32() + scanner.Size);

                return f_ptr;
            };

            var scanner2 = new SignatureScanner(state.GameProcess, ptr + 10, 0x1000);
            while ((ptr = scanner2.Scan(target)) != IntPtr.Zero
                && !found
                && scanner2.Size > 6)
            {
                scanner2 = new SignatureScanner(
                    state.GameProcess,
                    ptr,
                    scanner2.Address.ToInt32() + 0x1000 - ptr.ToInt32());
            }

            if (ptr == IntPtr.Zero)
                return;

            ptr = state.GameProcess.ReadPointer(ptr);
            target = new SigScanTarget(2, "80 ?? ?? ?? ?? 00 00 74");
            target.AddSignature(2, "8A ?? ?? ?? 00 00 84");

            scanner2 = new SignatureScanner(state.GameProcess, ptr, 0x100);
            ptr = scanner2.Scan(target);

            _nihiDeadOffset = state.GameProcess.ReadValue<int>(ptr);
            Debug.WriteLine("nihi dead bool offset is 0x" + _nihiDeadOffset.ToString("x"));
        }

        public override void OnSessionStart(GameState state)
        {
            base.OnSessionStart(state);
            if (IsLastMap)
            {
                IntPtr ptr = state.GetEntityByName("nihilanth");
                _nihiDead = new MemoryWatcher<bool>(ptr + _nihiDeadOffset);
            }
            _onceFlag = false;
        }


        public override GameSupportResult OnUpdate(GameState state)
        {
            if (_onceFlag)
                return GameSupportResult.DoNothing;

            if (this.IsLastMap)
            {
                _nihiDead.Update(state.GameProcess);

                if (!_nihiDead.Old && _nihiDead.Current)
                {
                    _onceFlag = true;
                    Debug.WriteLine("hls end");
                    return GameSupportResult.PlayerLostControl;
                }
            }

            return GameSupportResult.DoNothing;
        }
    }
}

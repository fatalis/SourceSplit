using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;

namespace LiveSplit.SourceSplit
{
    class GameMemory
    {
        [DllImport("winmm.dll")]
        static extern uint timeBeginPeriod(uint uMilliseconds);
        [DllImport("winmm.dll")]
        static extern uint timeEndPeriod(uint uMilliseconds);

        public event EventHandler<SetTimingMethodEventArgs> OnSetTimingMethod;
        public event EventHandler<SetTickRateEventArgs> OnSetTickRate;
        public event EventHandler<SessionTicksUpdateEventArgs> OnSessionTimeUpdate;
        public event EventHandler<PlayerControlChangedEventArgs> OnPlayerGainedControl;
        public event EventHandler<PlayerControlChangedEventArgs> OnPlayerLostControl;
        public event EventHandler<PlayerControlChangedEventArgs> ManualSplit;
        public event EventHandler<MapChangedEventArgs> OnMapChanged;
        public event EventHandler<SessionStartedEventArgs> OnSessionStarted;
        public event EventHandler<GamePausedEventArgs> OnGamePaused;
        public event EventHandler OnSessionEnded;
        public event EventHandler OnNewGameStarted;

        private Task _thread;
        private SynchronizationContext _uiThread;
        private CancellationTokenSource _cancelSource;

        private SigScanTarget _curTimeTarget;
        private SigScanTarget _signOnStateTarget1;
        private SigScanTarget _signOnStateTarget2;
        private SigScanTarget _curMapTarget;
        private SigScanTarget _globalEntityListTarget;
        private SigScanTarget _gameDirTarget;
        private SigScanTarget _hostStateTarget;
        private SigScanTarget _serverStateTarget;

        private SigScanTarget _infraIsLoadingTarget;
        private MemoryWatcher<byte> _infraIsLoading;
        private bool _isInfra = false;

        private bool _gotTickRate;

        private int _timesOver;
        private int _timeOverSpent;

        private SourceSplitSettings _settings;

        public GameState _state;

        // TODO: match tickrate as closely as possible without going over
        // otherwise we will most likely read when the game isn't sleeping
        // must also account for variance of windows scheduler
        private const int TARGET_UPDATE_RATE = 13;

        public GameMemory(SourceSplitSettings settings)
        {
            _settings = settings;

            // detect game offsets in a game/version-independent way by scanning for code signatures

            // TODO: refine hl2 2014 signatures once an update after the may 29th one is released

            _infraIsLoadingTarget = new SigScanTarget();

            // \x80\x3D\x2A\x2A\x2A\x2A\x00\x0F\x84\x2A\x2A\x2A\x2A\x56\xC6\x05\x2A\x2A\x2A\x2A\x00
            _infraIsLoadingTarget.AddSignature(2,
                "80 3D ?? ?? ?? ?? 00",     // CMP  loadingbyte,0x0
                "0F 84 ?? ?? ?? ??",        // JZ   0x10028ebb
                "56",                       // PUSH ESI
                "C6 05 ?? ?? ?? ?? 00");    // MOV  byte ptr [0x10458c04],0x0
            // \x80\x3D\x2A\x2A\x2A\x2A\x00\x0F\x84\x2A\x2A\x2A\x2A\x56\x57\xC6\x05\x2A\x2A\x2A\x2A\x00
            _infraIsLoadingTarget.AddSignature(2,
                "80 3D ?? ?? ?? ?? 00",     // CMP  loadingbyte,0x0
                "0F 84 ?? ?? ?? ??",        // JZ   0x1002b7db
                "56",                       // PUSH ESI
                "57",                       // PUSH EDI
                "C6 05 ?? ?? ?? ?? 00");    // MOV  byte ptr [0x1047ccd4],0x0

            // CBaseServer::(server_state_t)m_State
            _serverStateTarget = new SigScanTarget();
            _serverStateTarget.OnFound = (proc, scanner, ptr) => !proc.ReadPointer(ptr, out ptr) ? IntPtr.Zero : ptr;
            // works for every engine.dll
            // \x83\xf8\x01\x0f\x8c..\x00\x00\x3d\x00\x02\x00\x00\x0f\x8f..\x00\x00\x83\x3d(....)\x02\x7d
            _serverStateTarget.AddSignature(22,
                "83 F8 01",                // cmp     eax, 1
                "0F 8C ?? ?? 00 00",       // jl      loc_200087FB
                "3D 00 02 00 00",          // cmp     eax, 200h
                "0F 8F ?? ?? 00 00",       // jg      loc_200087FB
                "83 3d ?? ?? ?? ?? 02",    // cmp     m_State, 2
                "7D");                     // jge     short loc_200085FD

            // TODO: find a generic curTime signature
            // frameTime->curtime WIP sig, 76% success
            // \xe8...\x00\xd9\x1d....\x8b\x0d....\x8b\x11

            // CGlobalVarsBase::curtime (g_ClientGlobalVariables aka gpGlobals)
            _curTimeTarget = new SigScanTarget();
            _curTimeTarget.OnFound = (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr : IntPtr.Zero;
            // orange box and older
            // \xa3....\xb9....\xa3....\xe8....\xd9\x1d(....)\xb9....\xe8....\xd9\x1d
            _curTimeTarget.AddSignature(22,
                "A3 ?? ?? ?? ??",          // mov     dword_2038BA6C, eax
                "B9 ?? ?? ?? ??",          // mov     ecx, offset unk_2038B8E8
                "A3 ?? ?? ?? ??",          // mov     dword_2035DDA4, eax
                "E8 ?? ?? ?? ??",          // call    sub_20048110
                "D9 1D ?? ?? ?? ??",       // fstp    curTime
                "B9 ?? ?? ?? ??",          // mov     ecx, offset unk_2038B8E8
                "E8 ?? ?? ?? ??",          // call    sub_20048130
                "D9 1D");                  // fstp    frametime

            // portal 2
            // \x89\x96\xc4\x00\x00\x00\x8b\x86\xc8\x00\x00\x00\x8b\xce\xa3....\xe8....\xd9\x1d(....)\x8b\xce\xe8....\xd9\x1d
            _curTimeTarget.AddSignature(26,
                "89 96 C4 00 00 00",       // mov     [esi+0C4h], edx
                "8B 86 C8 00 00 00",       // mov     eax, [esi+0C8h]
                "8B CE",                   // mov     ecx, esi
                "A3 ?? ?? ?? ??",          // mov     dword_10414AD0, eax
                "E8 ?? ?? ?? ??",          // call    sub_100A0F30
                "D9 1D ?? ?? ?? ??",       // fstp    curTime
                "8B CE",                   // mov     ecx, esi
                "E8 ?? ?? ?? ??",          // call    sub_100A0FB0
                "D9 1D");                  // fstp    frametime

            // source 2009
            // \x89\x8f\xc4\x00\x00\x00\x8b\x97\xc8\x00\x00\x00\x8b\xcf\x89\x15....\xe8....\xd9\x1d(....)\x8b\xcf\xe8....\xd9\x1d
            _curTimeTarget.AddSignature(27,
                "89 8F C4 00 00 00",       // mov     [edi+0C4h], ecx
                "8B 97 C8 00 00 00",       // mov     edx, [edi+0C8h]
                "8B CF",                   // mov     ecx, edi
                "89 15 ?? ?? ?? ??",       // mov     dword_10422624, edx
                "E8 ?? ?? ?? ??",          // call    sub_1008FE40
                "D9 1D ?? ?? ?? ??",       // fstp    curTime
                "8B CF",                   // mov     ecx, edi
                "E8 ?? ?? ?? ??",          // call    sub_1008FEB0
                "D9 1D");                  // fstp    flt_1042261C

            // hl2 may 29 2014 update
            // \xa3....\x89\x15....\xe8....\xd9\x1d....\x57\xb9....\xe8....\x8b\x0d....\xd9\x1d
            _curTimeTarget.AddSignature(18,
                "A3 ?? ?? ?? ??",          // mov     dword_103B4AC8, eax
                "89 15 ?? ?? ?? ??",       // mov     dword_10452F38, edx
                "E8 ?? ?? ?? ??",          // call    sub_100CE610
                "D9 1D ?? ?? ?? ??",       // fstp    curTime
                "57",                      // push    edi
                "B9 ?? ?? ?? ??",          // mov     ecx, offset unk_10452D98
                "E8 ?? ?? ?? ??",          // call    sub_100CE390
                "8B 0D ?? ?? ?? ??",       // mov     ecx, dword_1043686C
                "D9 1D");                  // fstp    frametime
            // bms retail
            // \xa3....\x89\x15....\xe8....\xd9\x1d....\xb9....\xe8....\x8b\x0d....\xd9\x1d
            _curTimeTarget.AddSignature(18,
                "A3 ?? ?? ?? ??",          // mov     dword_103B4AC8, eax
                "89 15 ?? ?? ?? ??",       // mov     dword_10452F38, edx
                "E8 ?? ?? ?? ??",          // call    sub_100CE610
                "D9 1D ?? ?? ?? ??",       // fstp    curTime
                "B9 ?? ?? ?? ??",          // mov     ecx, offset unk_10452D98
                "E8 ?? ?? ?? ??",          // call    sub_100CE390
                "8B 0D ?? ?? ?? ??",       // mov     ecx, dword_1043686C
                "D9 1D");                  // fstp    frametime

            // CBaseClientState::m_nSignOnState (older engines)
            _signOnStateTarget1 = new SigScanTarget();
            _signOnStateTarget1.OnFound = (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr : IntPtr.Zero;
            // orange box and older (and bms retail)
            // \x80\x3d....\x00\x74\x06\xb8....\xc3\x83\x3d(....)\x02\xb8
            _signOnStateTarget1.AddSignature(17,
                "80 3D ?? ?? ?? ?? 00",    // cmp     byte_698EE114, 0
                "74 06",                   // jz      short loc_6936C8FF
                "B8 ?? ?? ?? ??",          // mov     eax, offset aDedicatedServe ; "Dedicated Server"
                "C3",                      // retn
                "83 3D ?? ?? ?? ?? 02",    // cmp     CBaseClientState__m_nSignonState, 2
                "B8 ?? ?? ?? ??");         // mov     eax, offset MultiByteStr

            // CBaseClientState::m_nSignOnState
            _signOnStateTarget2 = new SigScanTarget();
            _signOnStateTarget2.OnFound = (proc, scanner, ptr) => {
                if (!proc.ReadPointer(ptr, out ptr)) // deref instruction
                    return IntPtr.Zero;
                if (!proc.ReadPointer(ptr, out ptr)) // deref ptr
                    return IntPtr.Zero;
                return IntPtr.Add(ptr, 0x70); // this+0x70 = m_nSignOnState
            };
            // source 2009 / portal 2
            // \x74.\x8b\x74\x87\x04\x83\x7e\x18\x00\x74\x2d\x8b\x0d(....)\x8b\x49\x18
            _signOnStateTarget2.AddSignature(14,
                "74 ??",                   // jz      short loc_693D4E22
                "8B 74 87 04",             // mov     esi, [edi+eax*4+4]
                "83 7E 18 00",             // cmp     dword ptr [esi+18h], 0
                "74 2D",                   // jz      short loc_693D4DFC
                "8B 0D ?? ?? ?? ??",       // mov     ecx, baseclientstate
                "8B 49 18");               // mov     mov     ecx, [ecx+18h]

            // CBaseServer::m_szMapname[64]
            _curMapTarget = new SigScanTarget();
            _curMapTarget.OnFound = (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr : IntPtr.Zero;
            // \x68(....).\xe8...\x00\x83\xc4\x08\x85\xc0\x0f\x84..\x00\x00\x83\xc7\x01\x83.\x50\x3b\x7e\x18\x7c
            // source 2006 and older
            _curMapTarget.AddSignature(1,
                "68 ?? ?? ?? ??",          // push    offset map
                "??",                      // push    ebp
                "E8 ?? ?? ?? 00",          // call    __stricmp
                "83 C4 08",                // add     esp, 8
                "85 C0",                   // test    eax, eax
                "0F 84 ?? ?? 00 00",       // jz      loc_200CDF8D
                "83 C7 01",                // add     edi, 1
                "83 ?? 50",                // add     ebp, 50h
                "3B 7E 18",                // cmp     edi, [esi+18h]
                "7C");                     // jl      short loc_200CDEC0
            // orange box and newer
            // \xd9.\x2c\xd9\xc9\xdf\xf1\xdd\xd8\x76.\x80.....\x00
            _curMapTarget.AddSignature(13,
                "D9 ?? 2C",                // fld     dword ptr [edx+2Ch]
                "D9 C9",                   // fxch    st(1)
                "DF F1",                   // fcomip  st, st(1)
                "DD D8",                   // fstp    st
                "76 ??",                   // jbe     short loc_6946F651
                "80 ?? ?? ?? ?? ?? 00");   // cmp     map, 0
            // bms retail
            // \xdd.....\xdc.....\xdf\xf1\xdd\xd8\x76.\x80.....\x00
            _curMapTarget.AddSignature(20,
                "DD ?? ?? ?? ?? ??",       // fld     [ebp+var_144]
                "DC ?? ?? ?? ?? ??",       // fsub    dbl_103F36D8
                "DF F1",                   // fcomip  st, st(1)
                "DD D8",                   // fstp    st
                "76 ??",                   // jbe     short loc_101B8F6F
                "80 ?? ?? ?? ?? ?? 00");   // cmp     map, 0
            // infra
            _curMapTarget.AddSignature(16,
                "68 ?? ?? ?? ??",          // push    0x103603e0
                "c6 ?? ?? ??",             // mov     byte ptr [EBP + -0x1],0x1
                "ff ??",                   // call    ESI
                "83 c4 ??",                // add     ESP,0x4
                "80 ?? ?? ?? ?? ?? 00",    // cmp     map, 0x0
                "B8 ?? ?? ?? ??");         // mov     EAX, map

            // CBaseEntityList::(CEntInfo)m_EntPtrArray
            _globalEntityListTarget = new SigScanTarget();
            // \x6a\x00\x6a\x00\x50\x6a\x00\xb9(....)\xe8
            // deref to get vtable ptr, add 4 to get start of entity list
            _globalEntityListTarget.OnFound = (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr + 4 : IntPtr.Zero;
            _globalEntityListTarget.AddSignature(8,
                "6A 00",                   // push    0
                "6A 00",                   // push    0
                "50",                      // push    eax
                "6A 00",                   // push    0
                "B9 ?? ?? ?? ??",          // mov     ecx, offset CGlobalEntityList_vtable_ptr
                "E8");                     // call    sub_22289800

            // CHostState::m_currentState
            _hostStateTarget = new SigScanTarget();
            // subtract 4 to get m_currentState
            _hostStateTarget.OnFound = (proc, scanner, ptr) => proc.ReadPointer(ptr, out ptr) ? ptr - 4 : IntPtr.Zero;
            // \xc7\x05....\x07\x00\x00\x00\xc3
            _hostStateTarget.AddSignature(2,
                "C7 05 ?? ?? ?? ?? 07 00 00 00", // mov     g_HostState_m_nextState, 7
                "C3");                           // retn

            // TODO: find better way to do this. multiple sigs instead?
            _gameDirTarget = new SigScanTarget(0, "25732F736176652F25732E736176"); // "%s/save/%s.sav"
            _gameDirTarget.OnFound = (proc, scanner, ptr) => {
                byte[] b = BitConverter.GetBytes(ptr.ToInt32());
                var target = new SigScanTarget(-4,
                    // push    offset aSMapsS_sav
                    $"68 {b[0]:X02} {b[1]:X02} {b[2]:X02} {b[3]:X02}");
                IntPtr ptrPtr = scanner.Scan(target);
                if (ptrPtr == IntPtr.Zero)
                    return IntPtr.Zero;
                IntPtr ret;
                proc.ReadPointer(ptrPtr, out ret);
                return ret;
            };
        }

#if DEBUG
        ~GameMemory()
        {
            Debug.WriteLine("GameMemory finalizer");
        }
#endif
        public void StartReading()
        {
            if (_thread != null && _thread.Status == TaskStatus.Running)
                throw new InvalidOperationException();
            if (!(SynchronizationContext.Current is WindowsFormsSynchronizationContext))
                throw new InvalidOperationException("SynchronizationContext.Current is not a UI thread.");

            _cancelSource = new CancellationTokenSource();
            _uiThread = SynchronizationContext.Current;
            _thread = Task.Factory.StartNew(() => MemoryReadThread(_cancelSource));
        }

        public void Stop()
        {
            if (_cancelSource == null || _thread == null || _thread.Status != TaskStatus.Running)
                return;

            _cancelSource.Cancel();
            _thread.Wait();
        }

        void MemoryReadThread(CancellationTokenSource cts)
        {
            // force windows timer resolution to 1ms. it probably already is though, from the game.
            timeBeginPeriod(1);
            // we do a lot of timing critical stuff so this may help out
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

            while (true)
            {
                try
                {
                    Debug.WriteLine("Waiting for process");

                    Process game;
                    GameOffsets offsets;
                    while (!this.TryGetGameProcess(out game, out offsets))
                    {
                        Thread.Sleep(750);

                        if (cts.IsCancellationRequested)
                            goto ret;
                    }

                    this.HandleProcess(game, offsets, cts);

                    if (cts.IsCancellationRequested)
                        goto ret;
                }
                catch (Exception ex) // probably a Win32Exception on access denied to a process
                {
                    Trace.WriteLine(ex.ToString());
                    Thread.Sleep(1000);
                }
            }

        ret:

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            timeEndPeriod(1);
        }

        // TODO: log fails
        bool TryGetGameProcess(out Process p, out GameOffsets offsets)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            string[] procs = _settings.GameProcesses.Select(x => x.ToLower().Replace(".exe", String.Empty)).ToArray();
            p = Process.GetProcesses().FirstOrDefault(x => procs.Contains(x.ProcessName.ToLower()));
            offsets = new GameOffsets();

            if (p == null || p.HasExited || Util.IsVACProtectedProcess(p))
                return false;

            // process is up, check if engine and server are both loaded yet
            ProcessModuleWow64Safe engine = p.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "engine.dll");
            ProcessModuleWow64Safe server = p.ModulesWow64Safe().FirstOrDefault(x => x.ModuleName.ToLower() == "server.dll");

            if (engine == null || server == null)
                return false;

            // required engine stuff
            var scanner = new SignatureScanner(p, engine.BaseAddress, engine.ModuleMemorySize);

            if ((offsets.CurMapPtr = scanner.Scan(_curMapTarget)) == IntPtr.Zero
                || (offsets.CurTimePtr = scanner.Scan(_curTimeTarget)) == IntPtr.Zero
                || (offsets.GameDirPtr = scanner.Scan(_gameDirTarget)) == IntPtr.Zero
                || (offsets.HostStatePtr = scanner.Scan(_hostStateTarget)) == IntPtr.Zero
                || (offsets.ServerStatePtr = scanner.Scan(_serverStateTarget)) == IntPtr.Zero)
                return false;

            if ((offsets.SignOnStatePtr = scanner.Scan(_signOnStateTarget1)) == IntPtr.Zero
                && (offsets.SignOnStatePtr = scanner.Scan(_signOnStateTarget2)) == IntPtr.Zero)
                return false;

            _infraIsLoading = new MemoryWatcher<byte>(new DeepPointer(scanner.Scan(_infraIsLoadingTarget), 0x0));

            // required server stuff
            var serverScanner = new SignatureScanner(p, server.BaseAddress, server.ModuleMemorySize);

            if ((offsets.GlobalEntityListPtr = serverScanner.Scan(_globalEntityListTarget)) == IntPtr.Zero)
                return false;

            // entity offsets
            if ( !GetBaseEntityMemberOffset("m_fFlags", p, serverScanner, out offsets.BaseEntityFlagsOffset)
                || !GetBaseEntityMemberOffset("m_vecAbsOrigin", p, serverScanner, out offsets.BaseEntityAbsOriginOffset)
                || !GetBaseEntityMemberOffset("m_iName", p, serverScanner, out offsets.BaseEntityTargetNameOffset)
                || !GetBaseEntityMemberOffset("m_hViewEntity", p, serverScanner, out offsets.BasePlayerViewEntity))
                return false;

            // find m_pParent offset. the string "m_pParent" occurs more than once so we have to do something else
            // in old engine it's right before m_iParentAttachment. in new engine it's right before m_nTransmitStateOwnedCounter
            // TODO: test on all engines
            int tmp;
            if (!GetBaseEntityMemberOffset("m_nTransmitStateOwnedCounter", p, serverScanner, out tmp))
            {
                if (!GetBaseEntityMemberOffset("m_iParentAttachment", p, serverScanner, out tmp))
                    return false;
                tmp -= 4; // sizeof m_iParentAttachment
            }
            tmp -= 4; // sizeof m_nTransmitStateOwnedCounter (4 aligned byte)
            offsets.BaseEntityParentHandleOffset = tmp;

            Debug.WriteLine("CBaseServer::m_szMapname ptr = 0x" + offsets.CurMapPtr.ToString("X"));
            Debug.WriteLine("CGlobalVarsBase::curtime ptr = 0x" + offsets.CurTimePtr.ToString("X"));
            Debug.WriteLine("CBaseClientState::m_nSignonState ptr = 0x" + offsets.SignOnStatePtr.ToString("X"));
            Debug.WriteLine("CBaseEntityList::(CEntInfo)m_EntPtrArray ptr = 0x" + offsets.GlobalEntityListPtr.ToString("X"));
            Debug.WriteLine("CBaseEntity::m_fFlags offset = 0x" + offsets.BaseEntityFlagsOffset.ToString("X"));
            Debug.WriteLine("CBaseEntity::m_vecAbsOrigin offset = 0x" + offsets.BaseEntityAbsOriginOffset.ToString("X"));
            Debug.WriteLine("CBaseEntity::m_iName offset = 0x" + offsets.BaseEntityTargetNameOffset.ToString("X"));
            Debug.WriteLine("CBaseEntity::m_pParent offset = 0x" + offsets.BaseEntityParentHandleOffset.ToString("X"));
            Debug.WriteLine("CBasePlayer::m_hViewEntity offset = 0x" + offsets.BasePlayerViewEntity.ToString("X"));

#if DEBUG
            Debug.WriteLine("TryGetGameProcess took: " + sw.Elapsed);
#endif

            return true;
        }

        // also works for anything derived from CBaseEntity (player etc) (no multiple inheritance)
        // var must be included by one of the DEFINE_FIELD macros
        public static bool GetBaseEntityMemberOffset(string member, Process game, SignatureScanner scanner, out int offset)
        {
            offset = -1;

            IntPtr stringPtr = scanner.Scan(new SigScanTarget(0, Encoding.ASCII.GetBytes(member)));
            if (stringPtr == IntPtr.Zero)
                return false;

            var b = BitConverter.GetBytes(stringPtr.ToInt32());

            var target = new SigScanTarget(10,
                $"C7 05 ?? ?? ?? ?? {b[0]:X02} {b[1]:X02} {b[2]:X02} {b[3]:X02}"); // mov     dword_15E2BF1C, offset aM_fflags ; "m_fFlags"
            target.OnFound = (proc, s, ptr) => {
                // this instruction is almost always directly after above one, but there are a few cases where it isn't
                // so we have to scan down and find it
                var proximityScanner = new SignatureScanner(proc, ptr, 256);
                return proximityScanner.Scan(new SigScanTarget(6, "C7 05 ?? ?? ?? ?? ?? ?? 00 00"));         // mov     dword_15E2BF20, 0CCh
            };

            IntPtr addr = scanner.Scan(target);
            if (addr == IntPtr.Zero)
            {
                // seen in Black Mesa Source (legacy version)
                var target2 = new SigScanTarget(1,
                 "68 ?? ?? ?? ??",                                  // push    256
                $"68 {b[0]:X02} {b[1]:X02} {b[2]:X02} {b[3]:X02}"); // push    offset aM_fflags ; "m_fFlags"
                addr = scanner.Scan(target2);

                if (addr == IntPtr.Zero)
                    return false;
            }

            return game.ReadValue(addr, out offset);
        }

        void HandleProcess(Process game, GameOffsets offsets, CancellationTokenSource cts)
        {
            Debug.WriteLine("HandleProcess " + game.ProcessName);

            var state = new GameState(game, offsets);
            _state = state;
            this.InitGameState(state);
            _gotTickRate = false;

            var profiler = Stopwatch.StartNew();
            while (!game.HasExited && !cts.IsCancellationRequested)
            {
                // iteration must never take longer than 1 tick
                if (_isInfra)
                    _infraIsLoading.Update(state.GameProcess);

                this.UpdateGameState(state);
                this.CheckGameState(state);

                state.UpdateCount++;
                TimedTraceListener.Instance.UpdateCount = state.UpdateCount;

                if (profiler.ElapsedMilliseconds >= TARGET_UPDATE_RATE)
                {
                    _timesOver += 1;
                    _timeOverSpent += Convert.ToInt32(profiler.ElapsedMilliseconds) - TARGET_UPDATE_RATE;
                    Debug.WriteLine("**** update iteration took too long: " + profiler.ElapsedMilliseconds + "ms, times: " + _timesOver + ", total: " + _timeOverSpent + "ms");
                }

                //var sleep = Stopwatch.StartNew();
                //MapTimesForm.Instance.Text = profiler.Elapsed.ToString();
                Thread.Sleep(Math.Max(TARGET_UPDATE_RATE - (int)profiler.ElapsedMilliseconds, 1));
                //MapTimesForm.Instance.Text = sleep.Elapsed.ToString();
                profiler.Restart();
            }

            // if the game crashed, make sure session ends
            if (state.HostState == HostState.Run)
                this.SendSessionEndedEvent();
        }

        void InitGameState(GameState state)
        {
            string absoluteGameDir;
            state.GameProcess.ReadString(state.GameOffsets.GameDirPtr, ReadStringType.UTF8, 260, out absoluteGameDir);
            state.GameDir = new DirectoryInfo(absoluteGameDir).Name.ToLower();
            Debug.WriteLine("gameDir = " + state.GameDir);

            if (state.GameDir == "infra")
                _isInfra = true;

            state.CurrentMap = String.Empty;

            // inspect memory layout to determine CEntInfo's version
            const int SERIAL_MASK = 0x7FFF;
            int serial;
            state.GameProcess.ReadValue(state.GameOffsets.GlobalEntityListPtr + (4 * 7), out serial);
            state.GameOffsets.EntInfoSize = (serial > 0 && serial < SERIAL_MASK) ? CEntInfoSize.Portal2 : CEntInfoSize.HL2;

            state.GameSupport = GameSupport.FromGameDir(state.GameDir);
            if (state.GameSupport != null)
            {
                Debug.WriteLine("running game-specific code for: " + state.GameDir);
                state.GameSupport.OnGameAttached(state);
            }

            this.SendSetTimingMethodEvent(state.GameSupport?.GameTimingMethod ?? GameTimingMethod.EngineTicks);
        }

        void UpdateGameState(GameState state)
        {
            Process game = state.GameProcess;
            GameOffsets offsets = state.GameOffsets;

            // update all the stuff that doesn't depend on the signon state
            game.ReadValue(offsets.TickCountPtr, out state.RawTickCount);
            game.ReadValue(offsets.IntervalPerTickPtr, out state.IntervalPerTick);

            state.PrevSignOnState = state.SignOnState;

            // infra's signonstate is unreliable because it isn't updated on the "load" command and some others
            // so we'll have to settle with a loading byte
            if (_isInfra)
            {
                switch (_infraIsLoading.Current)
                {
                    case 0:
                        state.SignOnState = SignOnState.Full;
                        break;

                    case 1:
                        state.SignOnState = SignOnState.None;
                        break;
                }
            }

            game.ReadValue(offsets.SignOnStatePtr, out state.SignOnState);

            state.PrevHostState = state.HostState;
            game.ReadValue(offsets.HostStatePtr, out int hoststate);

            // in infra, hoststates above 1 are offset by 1
            if (_isInfra)
                state.HostState = (HostState)((hoststate > 1) ? hoststate - 1 : hoststate);
            else
                state.HostState = (HostState)hoststate;

            state.PrevServerState = state.ServerState;
            game.ReadValue(offsets.ServerStatePtr, out state.ServerState);

            bool firstTick = false;

            // update the stuff that's only valid during signon state full
            if (state.SignOnState == SignOnState.Full)
            {
                // if signon state just became full (where demos start timing from)
                if (state.SignOnState != state.PrevSignOnState)
                {
                    firstTick = true;

                    // start rebasing from this tick
                    state.TickBase = state.RawTickCount;
                    Debug.WriteLine("rebasing ticks from " + state.TickBase);

                    // player was just spawned, get it's ptr
                    state.PlayerEntInfo = state.GetEntInfoByIndex(GameState.ENT_INDEX_PLAYER);

                    // update map name
                    state.GameProcess.ReadString(state.GameOffsets.CurMapPtr, ReadStringType.ASCII, 64, out state.CurrentMap);
                }
                // in infra, when loading a save the current tick counter inherits the value stored in the save file
                // and continue incrementing from there, however sometimes immediately after load the counter gets reset to the inherited value
                // so we need to check if this happens
                if (_isInfra && state.RawTickCount - state.TickBase < 0)
                {
                    Debug.WriteLine("based ticks is wrong by " + (state.RawTickCount - state.TickBase) + " rebasing to " + state.TickBase);
                    state.TickBase = state.RawTickCount;
                }

                // update time and rebase it against the first signon state full tick
                state.TickCount = state.RawTickCount - state.TickBase;
                state.TickTime = state.TickCount * state.IntervalPerTick;
                TimedTraceListener.Instance.TickCount = state.TickCount;

                // update player related things
                if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero && state.GameSupport != null)
                {
                    // flags
                    if (state.GameSupport.RequiredProperties.HasFlag(PlayerProperties.Flags))
                    {
                        state.PrevPlayerFlags = state.PlayerFlags;
                        game.ReadValue(state.PlayerEntInfo.EntityPtr + offsets.BaseEntityFlagsOffset, out state.PlayerFlags);
                    }

                    // position
                    if (state.GameSupport.RequiredProperties.HasFlag(PlayerProperties.Position))
                    {
                        state.PrevPlayerPosition = state.PlayerPosition;
                        game.ReadValue(state.PlayerEntInfo.EntityPtr + offsets.BaseEntityAbsOriginOffset, out state.PlayerPosition);
                    }

                    // view entity
                    if (state.GameSupport.RequiredProperties.HasFlag(PlayerProperties.ViewEntity))
                    {
                        const int ENT_ENTRY_MASK = 0x7FF;

                        state.PrevPlayerViewEntityIndex = state.PlayerViewEntityIndex;
                        int viewEntityHandle; // EHANDLE
                        game.ReadValue(state.PlayerEntInfo.EntityPtr + offsets.BasePlayerViewEntity, out viewEntityHandle);
                        state.PlayerViewEntityIndex = viewEntityHandle == -1
                            ? GameState.ENT_INDEX_PLAYER
                            : viewEntityHandle & ENT_ENTRY_MASK;
                    }

                    // parent entity
                    if (state.GameSupport.RequiredProperties.HasFlag(PlayerProperties.ParentEntity))
                    {
                        state.PrevPlayerParentEntityHandle = state.PlayerParentEntityHandle; // EHANDLE
                        game.ReadValue(state.PlayerEntInfo.EntityPtr + offsets.BaseEntityParentHandleOffset, out state.PlayerParentEntityHandle);
                    }

                    // if it's the first tick, don't use stuff from the previous map
                    if (firstTick)
                    {
                        state.PrevPlayerFlags = state.PlayerFlags;
                        state.PrevPlayerPosition = state.PlayerPosition;
                        state.PrevPlayerViewEntityIndex = state.PlayerViewEntityIndex;
                        state.PrevPlayerParentEntityHandle = state.PlayerParentEntityHandle;
                    }
                }
            } // if (state.SignOnState == SignOnState.Full)
        }

        void CheckGameState(GameState state)
        {
            if (state.IntervalPerTick > 0 && !_gotTickRate)
            {
                _gotTickRate = true;
                this.SendSetTickRateEvent(state.IntervalPerTick);
            }

            if (state.SignOnState != state.PrevSignOnState)
                Debug.WriteLine("SignOnState changed to " + state.SignOnState);

            // if player is fully in game
            if (state.SignOnState == SignOnState.Full && state.HostState == HostState.Run)
            {
                // note: seems to be slow sometimes. ~3ms
                this.SendSessionTimeUpdateEvent(state.TickCount);

                // first tick when player is fully in game
                if (state.SignOnState != state.PrevSignOnState)
                {
                    Debug.WriteLine("session started");
                    this.SendSessionStartedEvent(state.CurrentMap);

                    state.GameSupport?.OnSessionStart(state);
                }

                if (state.ServerState == ServerState.Paused && state.PrevServerState == ServerState.Active)
                    this.SendGamePausedEvent(true);
                else if (state.ServerState == ServerState.Active && state.PrevServerState == ServerState.Paused)
                    this.SendGamePausedEvent(false);

                if (state.GameSupport != null)
                    this.HandleGameSupportResult(state.GameSupport.OnUpdate(state), state);

#if DEBUG
                if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
                    DebugPlayerState(state);
#endif
            }

            if (state.HostState != state.PrevHostState)
            {
                if (state.PrevHostState == HostState.Run)
                {
                    // the map changed or a quicksave was loaded
                    Debug.WriteLine("session ended");

                    // the map changed or a save was loaded
                    this.SendSessionEndedEvent();

                    state.GameSupport?.OnSessionEnd(state);
                }

                Debug.WriteLine("host state changed to " + state.HostState);

                // HostState::m_levelName is changed much earlier than state.CurrentMap (CBaseServer::m_szMapName)
                // reading HostStateLevelNamePtr is only valid during these states (not LoadGame!)
                if (state.HostState == HostState.ChangeLevelSP || state.HostState == HostState.ChangeLevelMP
                    || state.HostState == HostState.NewGame)
                {
                    string levelName;
                    state.GameProcess.ReadString(state.GameOffsets.HostStateLevelNamePtr, ReadStringType.ASCII, 256 - 1, out levelName);
                    Debug.WriteLine("host state m_levelName changed to " + levelName);

                    if (state.HostState == HostState.NewGame)
                    {
                        if (state.GameSupport != null && (levelName == state.GameSupport.FirstMap || levelName == state.GameSupport.FirstMap2))
                        {
                            this.SendNewGameStartedEvent(levelName);

                            if (state.GameSupport.StartOnFirstMapLoad)
                            {
                                // do a debug spew of the timer start
                                Debug.WriteLine(state.GameDir + " start");
                                this.HandleGameSupportResult(GameSupportResult.PlayerGainedControl, state);
                            }
                        }

                    }
                    else // changelevel sp/mp
                    {
                        // state.CurrentMap should still be the previous map
                        this.SendMapChangedEvent(levelName, state.CurrentMap);

                        // only for the beginner's guide, the "restart the level" option does a changelevel back to the currentmap rather than
                        // simply doing "restart"
                        // if the runner uses this option to reset at the first map then restart the timer
                        if (state.GameDir.ToLower() == "beginnersguide" &&
                            levelName == state.CurrentMap && state.CurrentMap == state.GameSupport.FirstMap)
                        {
                            Debug.WriteLine(state.GameDir + " start");
                            this.HandleGameSupportResult(GameSupportResult.PlayerGainedControl, state);
                        }
                    }
                }
            }
        }

        void HandleGameSupportResult(GameSupportResult result, GameState state)
        {
            if (result == GameSupportResult.DoNothing)
                return;

            switch (result)
            {
                case GameSupportResult.PlayerGainedControl:
                    this.SendGainedControlEvent(state.GameSupport.StartOffsetTicks);
                    break;
                case GameSupportResult.PlayerLostControl:
                    this.SendLostControlEvent(state.GameSupport.EndOffsetTicks);
                    break;
                case GameSupportResult.ManualSplit:
                    this.SendManualSplit(state.GameSupport.EndOffsetTicks);
                    break;
            }
        }

        // these functions are ugly but it means we don't have to worry about implicitly captured closures
        public void SendMapChangedEvent(string mapName, string prevMapName)
        {
            _uiThread.Post(d => {
                this.OnMapChanged?.Invoke(this, new MapChangedEventArgs(mapName, prevMapName));
            }, null);
        }

        public void SendSessionTimeUpdateEvent(int sessionTicks)
        {
            // note: sometimes this takes a few ms
            _uiThread.Post(d => {
                this.OnSessionTimeUpdate?.Invoke(this, new SessionTicksUpdateEventArgs(sessionTicks));
            }, null);
        }

        public void SendGainedControlEvent(int ticksOffset)
        {
            _uiThread.Post(d => {
                this.OnPlayerGainedControl?.Invoke(this, new PlayerControlChangedEventArgs(ticksOffset));
            }, null);
        }

        public void SendLostControlEvent(int ticksOffset)
        {
            _uiThread.Post(d => {
                this.OnPlayerLostControl?.Invoke(this, new PlayerControlChangedEventArgs(ticksOffset));
            }, null);
        }

        public void SendManualSplit(int ticksOffset)
        {
            _uiThread.Post(d => {
                this.ManualSplit?.Invoke(this, new PlayerControlChangedEventArgs(ticksOffset));
            }, null);
        }

        public void SendSessionStartedEvent(string map)
        {
            _uiThread.Post(d => {
                this.OnSessionStarted?.Invoke(this, new SessionStartedEventArgs(map));
            }, null);
        }

        public void SendSessionEndedEvent()
        {
            _uiThread.Post(d => {
                this.OnSessionEnded?.Invoke(this, EventArgs.Empty);
            }, null);
        }

        public void SendNewGameStartedEvent(string map)
        {
            _uiThread.Post(d => {
                this.OnNewGameStarted?.Invoke(this, EventArgs.Empty);
            }, null);
        }

        public void SendGamePausedEvent(bool paused)
        {
            _uiThread.Post(d => {
                this.OnGamePaused?.Invoke(this, new GamePausedEventArgs(paused));
            }, null);
        }

        public void SendSetTickRateEvent(float intervalPerTick)
        {
            _uiThread.Post(d => {
                this.OnSetTickRate?.Invoke(this, new SetTickRateEventArgs(intervalPerTick));
            }, null);
        }

        public void SendSetTimingMethodEvent(GameTimingMethod gameTimingMethod)
        {
            _uiThread.Post(d => {
                this.OnSetTimingMethod?.Invoke(this, new SetTimingMethodEventArgs(gameTimingMethod));
            }, null);
        }

#if DEBUG
        void DebugPlayerState(GameState state)
        {
            if (state.PlayerFlags != state.PrevPlayerFlags)
            {
                string addedList = String.Empty;
                string removedList = String.Empty;
                foreach (FL flag in Enum.GetValues(typeof(FL)))
                {
                    if (state.PlayerFlags.HasFlag(flag) && !state.PrevPlayerFlags.HasFlag(flag))
                        addedList += Enum.GetName(typeof(FL), flag) + " ";
                    else if (!state.PlayerFlags.HasFlag(flag) && state.PrevPlayerFlags.HasFlag(flag))
                        removedList += Enum.GetName(typeof(FL), flag) + " ";
                }
                if (addedList.Length > 0)
                    Debug.WriteLine("player flags added: " + addedList);
                if (removedList.Length > 0)
                    Debug.WriteLine("player flags removed: " + removedList);
            }

            if (state.PlayerViewEntityIndex != state.PrevPlayerViewEntityIndex)
            {
                Debug.WriteLine("player view entity changed: " + state.PlayerViewEntityIndex);
            }

            if (state.PlayerParentEntityHandle != state.PrevPlayerParentEntityHandle)
            {
                Debug.WriteLine("player parent entity changed: " + state.PlayerParentEntityHandle.ToString("X"));
            }

#if false
            if (!state.PlayerPosition.BitEquals(state.PrevPlayerPosition))
            {
                Debug.WriteLine("player pos changed: " + state.PlayerParentEntityHandle);
            }
#endif
        }
#endif
    }

    class MapChangedEventArgs : EventArgs
    {
        public string Map { get; private set; }
        public string PrevMap { get; private set; }
        public MapChangedEventArgs(string map, string prevMap)
        {
            this.Map = map;
            this.PrevMap = prevMap;
        }
    }

    class PlayerControlChangedEventArgs : EventArgs
    {
        public int TicksOffset { get; private set; }
        public PlayerControlChangedEventArgs(int ticksOffset)
        {
            this.TicksOffset = ticksOffset;
        }
    }

    class SessionTicksUpdateEventArgs : EventArgs
    {
        public int SessionTicks { get; private set; }
        public SessionTicksUpdateEventArgs(int sessionTicks)
        {
            this.SessionTicks = sessionTicks;
        }
    }

    class SessionStartedEventArgs : EventArgs
    {
        public string Map { get; private set; }
        public SessionStartedEventArgs(string map)
        {
            this.Map = map;
        }
    }

    class GamePausedEventArgs : EventArgs
    {
        public bool Paused { get; private set; }
        public GamePausedEventArgs(bool paused)
        {
            this.Paused = paused;
        }
    }

    class SetTickRateEventArgs : EventArgs
    {
        public float IntervalPerTick { get; private set; }
        public SetTickRateEventArgs(float intervalPerTick)
        {
            this.IntervalPerTick = intervalPerTick;
        }
    }

    class SetTimingMethodEventArgs : EventArgs
    {
        public GameTimingMethod GameTimingMethod { get; private set; }
        public SetTimingMethodEventArgs(GameTimingMethod gameTimingMethod)
        {
            this.GameTimingMethod = gameTimingMethod;
        }
    }

    public enum GameTimingMethod
    {
        EngineTicks,
        EngineTicksWithPauses,
        //RealTimeWithoutLoads
    }

    enum SignOnState
    {
        None = 0,
        Challenge = 1,
        Connected = 2,
        New = 3,
        PreSpawn = 4,
        Spawn = 5,
        Full = 6,
        ChangeLevel = 7
    }

    // HOSTSTATES
    enum HostState
    {
        NewGame = 0,
        LoadGame = 1,
        ChangeLevelSP = 2,
        ChangeLevelMP = 3,
        Run = 4,
        GameShutdown = 5,
        Shutdown = 6,
        Restart = 7
    }

    // server_state_t
    enum ServerState
    {
        Dead,
        Loading,
        Active,
        Paused
    }

    // MapLoadType_t
    enum MapLoadType
    {
        NewGame = 0,
        LoadGame = 1,
        Transition = 2,
        Background = 3
    }

    enum CEntInfoSize
    {
        HL2 = 4 * 4,
        Portal2 = 4 * 6
    }

    [Flags]
    public enum FL
    {
        //ONGROUND = (1<<0),
        //DUCKING = (1<<1),
        //WATERJUMP = (1<<2),
        ONTRAIN = (1 << 3),
        INRAIN = (1 << 4),
        FROZEN = (1 << 5),
        ATCONTROLS = (1 << 6),
        CLIENT = (1 << 7),
        FAKECLIENT = (1 << 8),
        //INWATER = (1<<9),
        FLY = (1 << 10),
        SWIM = (1 << 11),
        CONVEYOR = (1 << 12),
        NPC = (1 << 13),
        GODMODE = (1 << 14),
        NOTARGET = (1 << 15),
        AIMTARGET = (1 << 16),
        PARTIALGROUND = (1 << 17),
        STATICPROP = (1 << 18),
        GRAPHED = (1 << 19),
        GRENADE = (1 << 20),
        STEPMOVEMENT = (1 << 21),
        DONTTOUCH = (1 << 22),
        BASEVELOCITY = (1 << 23),
        WORLDBRUSH = (1 << 24),
        OBJECT = (1 << 25),
        KILLME = (1 << 26),
        ONFIRE = (1 << 27),
        DISSOLVING = (1 << 28),
        TRANSRAGDOLL = (1 << 29),
        UNBLOCKABLE_BY_PLAYER = (1 << 30)
    }
}

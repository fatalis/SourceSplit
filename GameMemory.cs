using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace LiveSplit.SourceSplit
{
    class GameMemory
    {
        public event SignOnStateChangeEventHandler OnSignOnStateChange;
        public delegate void SignOnStateChangeEventHandler(object sender, SignOnStateChangeEventArgs state);
        public event GameTimeUpdateEventHandler OnGameTimeUpdate;
        public delegate void GameTimeUpdateEventHandler(object sender, float gameTime);

        private string _curMap = String.Empty;
        public string CurrentMap
        {
            get {
                lock (_lock)
                    return _curMap;
            }
        }

        private Task _thread;
        private SynchronizationContext _callerThreadCtx;
        private CancellationTokenSource _cancelSource;
        private readonly object _lock = new object();

        //private SigScanTarget _serverStateTarget;
        private SigScanTarget _curTimeTarget;
        private SigScanTarget _signOnStateTarget1;
        private SigScanTarget _signOnStateTarget2;
        private SigScanTarget _curMapTarget;

        private SourceSplitSettings _settings;

        public GameMemory(SourceSplitSettings settings)
        {
            _settings = settings;
            
            /*// CBaseServer::(server_state_t)m_State
            _serverStateTarget = new SigScanTarget();
            _serverStateTarget.OnFound = (proc, ptr) => !ReadProcessPtr32(proc, ptr, out ptr) ? IntPtr.Zero : ptr;
            // works for every engine.dll
            // \x83\xf8\x01\x0f\x8c..\x00\x00\x3d\x00\x02\x00\x00\x0f\x8f..\x00\x00\x83\x3d(....)\x02\x7d
            _serverStateTarget.AddSignature(22,
                "83 F8 01",                // cmp     eax, 1
                "0F 8C ?? ?? 00 00",       // jl      loc_200087FB
                "3D 00 02 00 00",          // cmp     eax, 200h
                "0F 8F ?? ?? 00 00",       // jg      loc_200087FB
                "83 3d ?? ?? ?? ?? 02",    // cmp     m_State, 2
                "7D");                     // jge     short loc_200085FD*/

            // CGlobalVarsBase::curtime (g_ClientGlobalVariables aka gpGlobals)
            // hl2 old engine / portal latest / hl2 new engine
            _curTimeTarget = new SigScanTarget();
            _curTimeTarget.OnFound = (proc, ptr) => !ReadProcessPtr32(proc, ptr, out ptr) ? IntPtr.Zero : ptr;
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

            // dear esther / portal 2
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

            // l4d2
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

            // CBaseClientState::m_nSignOnState (older engines)
            _signOnStateTarget1 = new SigScanTarget();
            _signOnStateTarget1.OnFound = (proc, ptr) => !ReadProcessPtr32(proc, ptr, out ptr) ? IntPtr.Zero : ptr;
            // \x80\x3d....\x00\x74\x06\xb8....\xc3\x83\x3d(....)\x02\xb8....\x7c\x05
            _signOnStateTarget1.AddSignature(17,
                "80 3D ?? ?? ?? ?? 00",    // cmp     byte_698EE114, 0
                "74 06",                   // jz      short loc_6936C8FF
                "B8 ?? ?? ?? ??",          // mov     eax, offset aDedicatedServe ; "Dedicated Server"
                "C3",                      // retn
                "83 3D ?? ?? ?? ?? 02",    // cmp     CBaseClientState__m_nSignonState, 2
                "B8 ?? ?? ?? ??",          // mov     eax, offset MultiByteStr
                "7C 05");                  // jl      short locret_6936C912

            // CBaseClientState::m_nSignOnState (newer engines)
            _signOnStateTarget2 = new SigScanTarget();
            _signOnStateTarget2.OnFound = (proc, ptr) => {
                if (!ReadProcessPtr32(proc, ptr, out ptr)) // deref instruction
                    return IntPtr.Zero;
                if (!ReadProcessPtr32(proc, ptr, out ptr)) // deref ptr
                    return IntPtr.Zero;
                return IntPtr.Add(ptr, 0x70); // this+0x70 = m_nSignOnState
            };
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
            _curMapTarget.OnFound = (proc, ptr) => !ReadProcessPtr32(proc, ptr, out ptr) ? IntPtr.Zero : ptr;
            // TODO: these signatures arent very generic
            // \x68(....).\xe8...\x00\x83\xc4\x08\x85\xc0\x0f\x84..\x00\x00\x47\x83.\x50\x3b\x7e\x18\x7c
            _curMapTarget.AddSignature(1,
                "68 ?? ?? ?? ??",          // push    offset map
                "??",                      // push    ebx
                "E8 ?? ?? ?? 00",          // call    __stricmp
                "83 C4 08",                // add     esp, 8
                "85 C0",                   // test    eax, eax
                "0F 84 ?? ?? 00 00",       // jz      loc_6947E980
                "47",                      // inc     edi
                "83 ?? 50",                // add     ebx, 50h
                "3B 7E 18",                // cmp     edi, [esi+18h]
                "7C");                     // jl      short loc_6947E830
            // \x68(....).\xe8...\x00\x83\xc4\x08\x85\xc0\x0f\x84..\x00\x00\x83\xc7\x01\x83.\x50\x3b\x7e\x18\x7c
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
            // \x68(....).\xe8...\x00\x83\xc4\x08\x85\xc0\x0f\x84..\x00\x00\x47\x81.\xb0\x00\x00\x00\x3b\x7e\x18\x7c
            _curMapTarget.AddSignature(1,
                "68 ?? ?? ?? ??",          // push    offset map
                "??",                      // push    ebp
                "E8 ?? ?? ?? 00",          // call    __stricmp
                "83 C4 08",                // add     esp, 8
                "85 C0",                   // test    eax, eax
                "0F 84 ?? ?? 00 00",       // jz      loc_101B2BC1
                "47",                      // inc     edi
                "81 ?? B0 00 00 00",       // add     ebp, 0B0h
                "3B 7E 18",                // cmp     edi, [esi+18h]
                "7C");                     // jl      short loc_101B2A62
        }

        /// <summary>
        /// Begin reading the game's memory.
        /// </summary>
        public void StartReading()
        {
            if (_thread != null && _thread.Status == TaskStatus.Running)
                throw new InvalidOperationException();

            _callerThreadCtx = SynchronizationContext.Current;
            _cancelSource = new CancellationTokenSource();
            _thread = Task.Factory.StartNew(MemoryReadThread);
        }

        /// <summary>
        /// Stops reading the game's memory.
        /// </summary>
        public void Stop()
        {
            if (_cancelSource == null || _thread == null)
                throw new InvalidOperationException();

            if (_thread.Status != TaskStatus.Running)
                return;

            _cancelSource.Cancel();
            _thread.Wait();
        }

        /// <summary>
        /// Finds a game process with a known engine.dll.
        /// </summary>
        Process GetGameProcess(out IntPtr curMapPtr, out IntPtr curTimePtr, out IntPtr signOnStatePtr)
        {
            string[] procs = _settings.GameProcesses.Select(x => x.ToLower().Replace(".exe", String.Empty)).ToArray();
            Process p = Process.GetProcesses().FirstOrDefault(x => procs.Contains(x.ProcessName.ToLower()));

            if (p != null && !p.HasExited && !IsVACProtectedProcess(p))
            {
                ProcessModuleEx engine = GetProcessModules(p).FirstOrDefault(x => x.ModuleName.ToLower() == "engine.dll");
                if (engine != null)
                {
                    var scanner = new SignatureScanner(p, engine.BaseAddress, engine.ModuleMemorySize);
                    curMapPtr = scanner.Scan(_curMapTarget);
                    curTimePtr = scanner.Scan(_curTimeTarget);

                    signOnStatePtr = scanner.Scan(_signOnStateTarget1);
                    if (signOnStatePtr == IntPtr.Zero)
                        signOnStatePtr = scanner.Scan(_signOnStateTarget2);

                    if (curMapPtr != IntPtr.Zero && curTimePtr != IntPtr.Zero && signOnStatePtr != IntPtr.Zero)
                        return p;
                }

                Debug.WriteLine("Invalid process or unknown engine.dll");
            }

            curMapPtr = curTimePtr = signOnStatePtr = IntPtr.Zero;
            return null;
        }

        void MemoryReadThread()
        {
            while (!_cancelSource.IsCancellationRequested)
            {
                try
                {
                    Process gameProcess;
                    //IntPtr serverStatePtr;
                    IntPtr curTimePtr;
                    IntPtr signOnStatePtr;
                    IntPtr curMapPtr;

                    Debug.WriteLine("Waiting for process");

                    // wait for game process
                    while ((gameProcess = this.GetGameProcess(out curMapPtr, out curTimePtr, out signOnStatePtr)) == null)
                    {
                        Thread.Sleep(750);

                        if (_cancelSource.IsCancellationRequested)
                            return;
                    }

                    Debug.WriteLine("Got process " + gameProcess.ProcessName);

                    Debug.Assert(gameProcess != null && curMapPtr != IntPtr.Zero && curTimePtr != IntPtr.Zero && signOnStatePtr != IntPtr.Zero);

                    //IntPtr curMapNamePtr = IntPtr.Add(serverStatePtr, 13);
                    IntPtr tickCountPtr = IntPtr.Add(curTimePtr, 12);
                    IntPtr intervalPerTickPtr = IntPtr.Add(tickCountPtr, 4);
                    var mapName = new StringBuilder(64);
                    //GameState prevState = GameState.Dead;
                    SignOnState prevSignOnState = SignOnState.None;
                    float startTime = 0;
                    float prevTime = 0;
                    while (!gameProcess.HasExited)
                    {
                        //int st;
                        //float time;
                        int tickCount;
                        float intervalPerTick;
                        int sos;
                        //ReadProcessInt32(gameProcess, serverStatePtr, out st);
                        //ReadProcessFloat(gameProcess, curTimePtr, out time);
                        ReadProcessString(gameProcess, curMapPtr, mapName);
                        ReadProcessInt32(gameProcess, tickCountPtr, out tickCount);
                        ReadProcessFloat(gameProcess, intervalPerTickPtr, out intervalPerTick);
                        ReadProcessInt32(gameProcess, signOnStatePtr, out sos);
                        //GameState state = (GameState)st;
                        SignOnState signOnState = (SignOnState)sos;

                        float tickTime = tickCount*intervalPerTick;

                        if (signOnState != prevSignOnState)
                        {
                            if (signOnState == SignOnState.Full)
                            {
                                Debug.WriteLine("startTime set to " + tickTime);
                                startTime = tickTime;
                            }
                            else if (signOnState == SignOnState.None)
                                tickTime = prevTime;

                            // invoke on main thread
                            SignOnState prevStateClosure = prevSignOnState;
                            float startTimeClosure = startTime;
                            _callerThreadCtx.Send(s => {
                                if (this.OnSignOnStateChange != null)
                                    this.OnSignOnStateChange(this, new SignOnStateChangeEventArgs(signOnState, prevStateClosure,
                                        mapName.ToString(),
                                        tickTime - startTimeClosure));
                            }, null);

                            //Debug.WriteLine("state=" + signOnState + " " + tickTime + " " + mapName);
                        }

                        if (signOnState == SignOnState.Connected)
                            Debug.WriteLine("state=" + signOnState + " " + tickTime + " " + mapName);

                        if (signOnState == SignOnState.Full)
                        {
                            if (this.OnGameTimeUpdate != null)
                                this.OnGameTimeUpdate(this, tickTime - startTime);
                        }

                        lock (_lock)
                            _curMap = mapName.ToString();
                        //prevState = state;
                        prevSignOnState = signOnState;
                        prevTime = tickTime;

                        Thread.Sleep(15); // 66 tickrate
                        if (_cancelSource.IsCancellationRequested)
                            return;
                    }
                }
                catch (Exception ex) // probably a Win32Exception on access denied to a process
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
        }

        public static bool ReadProcessInt32(Process process, IntPtr addr, out int val)
        {
            byte[] bytes = new byte[4];
            int read;
            val = 0;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val = BitConverter.ToInt32(bytes, 0);
            return true;
        }

        public static bool ReadProcessPtr32(Process process, IntPtr addr, out IntPtr val)
        {
            byte[] bytes = new byte[4];
            int read;
            val = IntPtr.Zero;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val = (IntPtr)BitConverter.ToInt32(bytes, 0);
            return true;
        }

        public static bool ReadProcessFloat(Process process, IntPtr addr, out float val)
        {
            byte[] bytes = new byte[4];
            int read;
            val = 0;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val = BitConverter.ToSingle(bytes, 0);
            return true;
        }

        public static bool ReadProcessString(Process process, IntPtr addr, StringBuilder sb)
        {
            byte[] bytes = new byte[sb.Capacity];
            int read;
            sb.Clear();
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            sb.Append(Encoding.ASCII.GetString(bytes));
            sb.Replace("\0", String.Empty);
            return true;
        }

        /// <summary>
        /// Checks if the process is associated with a VAC protected game.
        /// We don't want to touch them. Even though reading a VAC process'
        /// memory is said to be perfectly fine and only writing is bad.
        /// </summary>
        static bool IsVACProtectedProcess(Process p)
        {
            // http://forums.steampowered.com/forums/showthread.php?t=2465755
            // http://en.wikipedia.org/wiki/Valve_Anti-Cheat#Games_that_support_VAC
            string[] badExes = { "csgo", "dota2", "swarm", "left4dead",
                "left4dead2", "dinodday", "insurgency", "nucleardawn", "ship" };
            string[] badMods = { "cstrike", "dods", "hl2mp", "insurgency", "tf", "zps" };

            if (badExes.Contains(p.ProcessName.ToLower()))
                return true;

            if (p.ProcessName.ToLower() == "hl2")
            {
                // it's too difficult to get another process' start arguments, so let's scan the dir
                // http://stackoverflow.com/questions/440932/reading-command-line-arguments-of-another-process-win32-c-code

                try
                {
                    string dir = Path.GetDirectoryName(p.MainModule.FileName);
                    if (dir == null)
                        return true;
                    foreach (DirectoryInfo di in new DirectoryInfo(dir).GetDirectories())
                    {
                        if (badMods.Contains(di.Name.ToLower()))
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    return true;
                }
            }

            return false;
        }

        static ProcessModuleEx[] ModuleToModuleEx(Process p)
        {
            var ret = new List<ProcessModuleEx>();
            foreach (ProcessModule module in p.Modules)
            {
                var ex = new ProcessModuleEx {
                    BaseAddress = module.BaseAddress,
                    EntryPointAddress = module.EntryPointAddress,
                    FileName = module.FileName,
                    ModuleMemorySize = module.ModuleMemorySize,
                    ModuleName = module.ModuleName
                };
                ret.Add(ex);
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Get the modules that have been loaded by the associated process.
        /// This will get an x86 process' modules when running from x64 code,
        /// unlike Process.Modules.
        /// </summary>
        static ProcessModuleEx[] GetProcessModules(Process p)
        {
            if (p.HasExited)
                throw new ArgumentException("Process should be alive.");
            if (!Environment.Is64BitProcess)
                return ModuleToModuleEx(p);
            
            var ret = new List<ProcessModuleEx>();
            
            IntPtr[] hMods = new IntPtr[1024];

            uint uiSize = (uint)(IntPtr.Size * hMods.Length);
            uint cbNeeded;
            try
            {
                const int LIST_MODULES_ALL = 3;
                if (!SafeNativeMethods.EnumProcessModulesEx(p.Handle, hMods, uiSize, out cbNeeded, LIST_MODULES_ALL))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
            catch (EntryPointNotFoundException) // this function is only on vista and higher. this is likely only to happen on XP x64
            {
                return ModuleToModuleEx(p); // fall back
            }

            uint count = (uint)(cbNeeded / IntPtr.Size);
            for (int i = 0; i < count; i++)
            {
                var info = new SafeNativeMethods.MODULEINFO();
                var path = new StringBuilder(260);
                var module = new ProcessModuleEx();

                if (SafeNativeMethods.GetModuleFileNameEx(p.Handle, hMods[i], path, path.Capacity) > 0)
                {
                    module.FileName = path.ToString();
                    module.ModuleName = Path.GetFileName(module.FileName);
                }

                if (SafeNativeMethods.GetModuleInformation(p.Handle, hMods[i], out info, (uint)Marshal.SizeOf(info)))
                {
                    module.BaseAddress = info.lpBaseOfDll;
                    module.EntryPointAddress = info.EntryPoint;
                    module.ModuleMemorySize = (int)info.SizeOfImage;
                    ret.Add(module);
                }
            }

            return ret.ToArray();
        }
    }

    // server_state_t
    internal enum GameState
    {
        Dead,
        Loading,
        Active,
        Paused
    };

    internal enum SignOnState
    {
        None = 0,
        Challenge = 1,
        Connected = 2,
        New = 3,
        PreSpawn = 4,
        Spawn = 5,
        Full = 6,
        ChangeLevel = 7
    };

    class SignOnStateChangeEventArgs : EventArgs
    {
        public string Map { get; set; }
        public SignOnState SignOnState { get; set; }
        public SignOnState PrevSignOnState { get; set; }
        public float GameTime { get; set; }

        public SignOnStateChangeEventArgs(SignOnState state, SignOnState prevState, string map, float gameTime)
        {
            this.SignOnState = state;
            this.PrevSignOnState = prevState;
            this.Map = map;
            this.GameTime = gameTime;
        }
    }

    class ProcessModuleEx
    {
        public IntPtr BaseAddress { get; set; }
        public IntPtr EntryPointAddress { get; set; }
        public string FileName { get; set; }
        public int ModuleMemorySize { get; set; }
        public string ModuleName { get; set; }

        public ProcessModuleEx()
        {
            this.FileName = String.Empty;
            this.ModuleName = String.Empty;
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.Extensions;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit
{
    // custom command system for games that need their own specific settings
    // usually set through monitoring a buffer for invalid console command inputs
    class CustomCommand
    {
        public string Name;
        public string Description;
        public bool Enabled { get; set; }

        public CustomCommand(string name, string description = "", bool enabled = false)
        {
            Name = name;
            Enabled = enabled;
            Description = description;
        }

        public void Update(bool enabled)
        {
            Enabled = enabled;
            Debug.WriteLine($"{Name} is {Enabled}");
        }

        public override string ToString()
        {
            return $"{Name} [{Enabled}] {(!string.IsNullOrEmpty(Description) ? (" - " + Description) : "")}";
        }
    }

    class CustomCommandHandler
    {
        public CustomCommand[] Commands { get; set; }
        private IntPtr _cmdBufferPtr = IntPtr.Zero;
        private IntPtr _cmdExecPtr = IntPtr.Zero;
        private StringWatcher _cmdBuffer;
        private RemoteOpsHandler _remoteOps;

        public CustomCommandHandler(params CustomCommand[] commands)
        {
            Commands = commands;
        }

        public void Init(GameState state)
        {
            _remoteOps = new RemoteOpsHandler(state.GameProcess);

            _cmdBufferPtr = IntPtr.Zero;
            ProcessModuleWow64Safe server = state.GetModule("server.dll");
            if (server == null)
            {
                Debug.WriteLine("Failed to initialize custom command handler!");
                return;
            }

            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            var commandTarg = new SigScanTarget(16, "55 8B EC 8D 45 ?? 50 FF 75 ?? 68 00 04 00 00 68 ?? ?? ?? ??");
            _cmdBufferPtr = scanner.Scan(commandTarg);
            if (_cmdBufferPtr == IntPtr.Zero)
            {
                commandTarg = new SigScanTarget(1, ("\0%s_weapon\0").ConvertToHex());
                SigScanTarget funcTarg = new SigScanTarget(5, "68" + scanner.Scan(commandTarg).GetByteString() + "E8");
                funcTarg.OnFound = (f_proc, f_scanner, f_ptr) =>
                    f_scanner.ReadCall(f_ptr);

                IntPtr tmp = scanner.Scan(funcTarg);
                if (tmp == IntPtr.Zero)
                    return;
                commandTarg = new SigScanTarget(6, "68 ?? ?? ?? 00 68");
                commandTarg.OnFound = (f_proc, f_scanner, f_ptr) => f_proc.ReadPointer(f_ptr);
                SignatureScanner newScanner = new SignatureScanner(state.GameProcess, tmp, 0x100);
                _cmdBufferPtr = newScanner.Scan(commandTarg);
            }
            else
                _cmdBufferPtr = state.GameProcess.ReadPointer(_cmdBufferPtr);

            if (_cmdBufferPtr != IntPtr.Zero)
                _cmdBuffer = new StringWatcher(_cmdBufferPtr + 0x11, 256);

            GetExecPtr(state);
            Update(state, true);

            SendConsoleMsg("");
            SendConsoleMsg("SourceSplit Custom Commands are present, enter \"ss_list\" to list them, or \"ss_h\" for help!");
            SendConsoleMsg("");

        }

        private void GetExecPtr(GameState state)
        {
            ProcessModuleWow64Safe engine = state.GetModule("engine.dll");
            var scanner = new SignatureScanner(state.GameProcess, engine.BaseAddress, engine.ModuleMemorySize);

            var target = new SigScanTarget(0, ("exec config_default.cfg").ConvertToHex());
            target.OnFound = (f_proc, f_scanner, f_ptr) =>
            {
                SigScanTarget newTarg = new SigScanTarget(0, $"68 {f_ptr.GetByteString()}");
                return f_scanner.Scan(newTarg);
            };

            _cmdExecPtr = scanner.ReadCall(scanner.Scan(target) + 0x5);
        }

        // allow disabling and enabling of features through monitoring specific console input
        // format: ebend<arg>, xenstart<arg>, eg: ebend1, xenstart0, characters are also accepted which will be interpreted as true
        public void Update(GameState state, bool ignoreChanged = false)
        {
            if (_cmdBufferPtr == IntPtr.Zero)
                return;

            _cmdBuffer.Update(state.GameProcess);
            if (ignoreChanged || _cmdBuffer.Changed)
            {
                if (string.IsNullOrEmpty(_cmdBuffer.Current))
                    return;

                try
                {
                    string cleanedCmd = _cmdBuffer.Current.Trim('\r', '\n').ToLower();
                    switch(cleanedCmd)
                    {
                        case "ss_list":
                            ListAllCommands();
                            return;
                        case "ss_h":
                            PrintHelp();
                            return;

                    }

                    foreach (CustomCommand cmd in Commands)
                    {
                        if (cleanedCmd.Contains(cmd.Name))
                        {
                            if (cleanedCmd.Length > cmd.Name.Length)
                            {
                                string arg = cleanedCmd.Substring(cleanedCmd.IndexOf(cmd.Name) + cmd.Name.Length, 1);
                                if (cmd.Enabled != (arg != "0"))
                                {
                                    SystemSounds.Asterisk.Play();
                                    cmd.Update(arg != "0");
                                    SendConsoleMsg(cmd.Name + " is " + (cmd.Enabled ? "Enabled" : "Disabled"));
                                }
                            }
                            else if (cleanedCmd == cmd.Name && !string.IsNullOrEmpty(cmd.Description))
                                SendConsoleMsg(cmd.ToString());
                        }
                    }
                }
                finally { state.GameProcess.WriteBytes(_cmdBufferPtr + 0x11, new byte[] { 0x69, 0x42, 0x00 }); }
            }
        }

        public void SendConsoleMsg(string cmd)
        {
            if (_cmdExecPtr == IntPtr.Zero)
                return;

            cmd = "echo " + cmd;
            _remoteOps.CallFunctionString(cmd, _cmdExecPtr);
        }

        private void ListAllCommands()
        {
            SendConsoleMsg("SourceSplit commands:");
            foreach (var cmd in Commands)
                SendConsoleMsg(cmd.ToString());
        }

        private void PrintHelp()
        {
            SendConsoleMsg("SourceSplit Custom Commands help:");
            SendConsoleMsg("- Type <command><1/0> to enable / disable funtions!");
            string name = Commands.Count() > 0 ? Commands[0].Name : "function";
            SendConsoleMsg("For example: " + $"{name}0 to disable {name}, {name}1 to enable");
            SendConsoleMsg("- You should hear a Windows Warning Sound and a Console message when a function is toggled.");

        }
    }
}

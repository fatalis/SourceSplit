using System;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.Extensions;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace LiveSplit.SourceSplit
{
    // custom command system for games that need their own specific settings
    // usually set through monitoring a buffer for invalid console command inputs
    class CustomCommand
    {
        public string Name;
        public string Description;
        public bool BValue { get; set; }
        public string Value { get; set; }
        public int IValue { get; set; }
        public float FValue { get; set; }
        private Action _callback = null;

        private static string[] _noVars = new string[] { "no", "0", "false" };

        public CustomCommand(string name, string def, string description = "", Action callback = null)
        {
            Name = name;
            Parse(def);
            Description = description;
            _callback = callback;
        }

        public bool Update(string input)
        {
            string[] elems = input.Split(' ');

            if (elems.Count() == 0 || elems[0] != Name)
                return false;

            input = input.Substring(Name.Length + 1);
            Parse(input);
            _callback?.Invoke();
            SystemSounds.Asterisk.Play();

            return true;
        }

        public void Parse(string input)
        {
            Value = input;
            BValue = !_noVars.Contains(input.Trim().ToLower());
            if (int.TryParse(input, out int tmpI))
                IValue = tmpI;
            else IValue = 0;
            if (float.TryParse(input, out float tmpF))
                FValue = tmpF;
            else FValue = 0;
        }

        public override string ToString()
        {
            return $"{Name} s[{Value}] b[{BValue}] i[{IValue}] f[{FValue}]\n{(!string.IsNullOrEmpty(Description) ? (" - " + Description) : "")}\n";
        }
    }

    class CustomCommandHandler
    {
        public CustomCommand[] Commands { get; set; }
        private IntPtr _cmdBufferPtr = IntPtr.Zero;
        private IntPtr _cmdExecPtr = IntPtr.Zero;
        private byte[] _cmdBuffer;
        private RemoteOpsHandler _remoteOps;
        private const int _bufferSize = 512;

        public CustomCommandHandler(params CustomCommand[] commands)
        {
            Commands = commands;
            _cmdBuffer = new byte[_bufferSize];
        }

        public void Init(GameState state)
        {
            _remoteOps = new RemoteOpsHandler(state.GameProcess);

            _cmdBufferPtr = IntPtr.Zero;
            ProcessModuleWow64Safe engine = state.GetModule("engine.dll");

            var scanner = new SignatureScanner(state.GameProcess, engine.BaseAddress, engine.ModuleMemorySize);
            IntPtr ptr = scanner.Scan(new SigScanTarget("68" + scanner.Scan(new SigScanTarget("execing %s\n".ConvertToHex())).GetByteString()));

            if (ptr == IntPtr.Zero)
                goto fail;

            byte[] bytes = state.GameProcess.ReadBytes(ptr, 100);
            for (int i = 0; i < 100; i++)
            {
                if (bytes[i] == 0xA1 || bytes[i] == 0xB9)
                {
                    uint val = state.GameProcess.ReadValue<uint>(ptr + i + 1);
                    if (scanner.IsWithin(val))
                    {
                        _cmdBufferPtr = (IntPtr)val;
                        Debug.WriteLine("Command buffer found at 0x" + _cmdBufferPtr.ToString("X"));
                        break;
                    }
                }
            }

            GetExecPtr(state);
            Update(state);
            SendConsoleMsg("\nSourceSplit Custom Commands are present, enter \"ss_list\" to list them, or \"ss_h\" for help!\n");

            return;

            fail:
            _cmdBufferPtr = IntPtr.Zero;
            Debug.WriteLine("Failed to initialize custom command handler!");
            return;
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
        public void Update(GameState state)
        {
            if (_cmdBufferPtr == IntPtr.Zero)
                return;

            byte[] newBuffer = state.GameProcess.ReadBytes(_cmdBufferPtr, _bufferSize);

            if (!newBuffer.SequenceEqual(_cmdBuffer))
            {
                int tmp = 0;
                for (int i = 0; i < _bufferSize; i++)
                {
                    // null byte, we've hit the end of a command
                    if (newBuffer[i] == 0x00)
                    {
                        int count = i - tmp + 1;
                        string cmd = Encoding.Default.GetString(newBuffer.Skip(tmp).Take(count).ToArray());

                        byte[] prevBytes = state.GameProcess.ReadBytes(_cmdBufferPtr + tmp, count);

                        if (ProcessCommand(cmd))
                            // don't modify the buffer if this section has changed
                            if (prevBytes.SequenceEqual(state.GameProcess.ReadBytes(_cmdBufferPtr + tmp, count)))
                                // remove the command from the buffer, replacing it with null bytes so we don't encounter
                                // it in the next update loop
                                state.GameProcess.WriteBytes(_cmdBufferPtr + tmp, new byte[count]);

                        tmp = i;

                        // 2nd null byte, we've hit the effective end of the buffer
                        if (newBuffer[i + 1] == 0x00)
                            break;
                    }
                }

                state.GameProcess.ReadBytes(_cmdBufferPtr, _bufferSize).CopyTo(_cmdBuffer, 0);
            }
        }

        private bool ProcessCommand(string input)
        {
            string cleanedCmd = input.ToLower().Trim(' ', '\0');
            switch (cleanedCmd)
            {
                case "ss_list":
                    ListAllCommands();
                    return true;
                case "ss_h":
                    PrintHelp();
                    return true;

            }
            foreach (CustomCommand cmd in Commands)
            {
                if (cleanedCmd.Contains(cmd.Name))
                {
                    if (cleanedCmd.Length > cmd.Name.Length && cmd.Update(cleanedCmd))
                        return true;
                    if (cleanedCmd == cmd.Name && !string.IsNullOrEmpty(cmd.Description))
                    {
                        SendConsoleMsg(cmd.ToString());
                        return true;
                    }
                }
            }

            return false;
        }

        public void SendConsoleMsg(string input)
        {
            if (_cmdExecPtr == IntPtr.Zero)
                return;

            List<string> commands = input.Split('\n').ToList();
            if (commands.Count() == 0)
                commands.Add(input);

            foreach (string command in commands)
            {
                string cmd = "echo " + command;
                _remoteOps.CallFunctionString(cmd, _cmdExecPtr);
                //_cmdExecBuffer.Add(cmd);
            }
        }

        private void ListAllCommands()
        {
            SendConsoleMsg("\nSourceSplit commands:\n");
            foreach (var cmd in Commands)
                SendConsoleMsg(cmd.ToString());
        }

        private void PrintHelp()
        {
            SendConsoleMsg(
                "\nSourceSplit Custom Commands help:\n" +
                "- Type <command> <1/0> to enable / disable functions!");
            string name = Commands.Count() > 0 ? Commands[0].Name : "function";
            SendConsoleMsg(
                "For example: " + $"{name} 0 to disable {name}, {name} 1 to enable\n" +
                $"- You should hear a Windows Warning Sound and a see Console message when a function is toggled.\n"
                /*$"WARNING: If you are executing multiple commands, please put a wait in between. This is due to limitations with detection.\n"*/);

        }
    }
}

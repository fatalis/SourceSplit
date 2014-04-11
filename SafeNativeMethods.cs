using System;
using System.Text;
using System.Runtime.InteropServices;

namespace LiveSplit.SourceSplit
{
    static class SafeNativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize, // should be IntPtr if we ever need to read a size bigger than 32 bit address space
            out int lpNumberOfBytesRead);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.FunctionPtr)] [In][Out] IntPtr[] lphModule,
            uint cb,
            [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded,
            uint dwFiltrFlag);

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }
        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [In][Out] StringBuilder lpBaseName, int nSize);
    }
}

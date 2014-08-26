using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LiveSplit.SourceSplit
{
    public class Util
    {
        /// <summary>
        /// Checks if the process is associated with a VAC protected game.
        /// We don't want to touch them. Even though reading a VAC process'
        /// memory is said to be perfectly fine and only writing is bad.
        /// </summary>
        public static bool IsVACProtectedProcess(Process p)
        {
            // http://forums.steampowered.com/forums/showthread.php?t=2465755
            // http://en.wikipedia.org/wiki/Valve_Anti-Cheat#Games_that_support_VAC
            string[] badExes = { "csgo", "dota2", "swarm", "left4dead",
                "left4dead2", "dinodday", "insurgency", "nucleardawn", "ship" };
            string[] badMods = { "cstrike", "dods", "hl2mp", "insurgency", "tf", "zps" };
            string[] badRootDirs = { "Dark Messiah of Might and Magic Multi-Player" };

            if (badExes.Contains(p.ProcessName.ToLower()))
                return true;

            if (p.ProcessName.ToLower() == "hl2" || p.ProcessName.ToLower() == "mm")
            {
                // it's too difficult to get another process' start arguments, so let's scan the dir
                // http://stackoverflow.com/questions/440932/reading-command-line-arguments-of-another-process-win32-c-code

                try
                {
                    string dir = Path.GetDirectoryName(p.MainModule.FileName);
                    if (dir == null)
                        return true;

                    if (new DirectoryInfo(dir).GetDirectories().Any(di => badMods.Contains(di.Name.ToLower())))
                        return true;

                    string root = new DirectoryInfo(dir).Name.ToLower();
                    if (badRootDirs.Any(badRoot => badRoot.ToLower() == root))
                        return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    return true;
                }
            }

            return false;
        }
    }

    public static class Extensions
    {
        public static unsafe float ToFloatBits(this uint i)
        {
            return *((float*)&i);
        }

        public static unsafe uint ToUInt32Bits(this float f)
        {
            return *((uint*)&f);
        }

        public static bool BitEquals(this float f, float o)
        {
            return ToUInt32Bits(f) == ToUInt32Bits(o);
        }

        public static bool ReadBytes(this Process process, IntPtr addr, byte[] bytes)
        {
            int read;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            return true;
        }

        public static bool ReadInt32(this Process process, IntPtr addr, out int val)
        {
            byte[] bytes = new byte[4];
            int read;
            val = 0;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val = BitConverter.ToInt32(bytes, 0);
            return true;
        }

        public static bool ReadBool(this Process process, IntPtr addr, out bool val)
        {
            byte[] bytes = new byte[1];
            int read;
            val = false;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val = bytes[0] != 0;
            return true;
        }

        public static bool ReadPtr32(this Process process, IntPtr addr, out IntPtr val)
        {
            byte[] bytes = new byte[4];
            int read;
            val = IntPtr.Zero;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val = (IntPtr)BitConverter.ToInt32(bytes, 0);
            return true;
        }

        public static bool ReadFloat(this Process process, IntPtr addr, out float val)
        {
            byte[] bytes = new byte[4];
            int read;
            val = 0;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val = BitConverter.ToSingle(bytes, 0);
            return true;
        }

        public static bool ReadVector3f(this Process process, IntPtr addr, out Vector3f val)
        {
            byte[] bytes = new byte[4 * 3];
            int read;
            val = new Vector3f();
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
                return false;
            val.X = BitConverter.ToSingle(bytes, 4 * 0);
            val.Y = BitConverter.ToSingle(bytes, 4 * 1);
            val.Z = BitConverter.ToSingle(bytes, 4 * 2);
            return true;
        }

        // tested perf, it's fine
        public static bool ReadStruct<T>(this Process process, IntPtr addr, out T val) where T : struct
        {
            byte[] bytes = new byte[Marshal.SizeOf(typeof(T))];
            int read;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
            {
                val = default(T);
                return false;
            }

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            val = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return true;
        }

        public static bool ReadEnum32<T>(this Process process, IntPtr addr, out T val) where T : struct
        {
            byte[] bytes = new byte[4];
            int read;
            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
            {
                val = default(T);
                return false;
            }
            val = (T)(object)BitConverter.ToInt32(bytes, 0);
            return true;
        }

        public static bool ReadASCIIString(this Process process, IntPtr addr, out string str, int max)
        {
            byte[] bytes = new byte[max];
            int read;

            if (!SafeNativeMethods.ReadProcessMemory(process.Handle, addr, bytes, bytes.Length, out read) || read != bytes.Length)
            {
                str = String.Empty;
                return false;
            }

            str = Encoding.ASCII.GetString(bytes);

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\0')
                {
                    str = str.Remove(i, str.Length - i);
                    break;
                }
            }

            return true;
        }
    }

    public struct Vector3f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public int IX { get { return (int)this.X; } }
        public int IY { get { return (int)this.Y; } }
        public int IZ { get { return (int)this.Z; } }

        // public Vector3f() {}

        public Vector3f(float x, float y, float z) : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3f(uint bitX, uint bitY, uint bitZ) : this()
        {
            this.X = bitX.ToFloatBits();
            this.Y = bitY.ToFloatBits();
            this.Z = bitZ.ToFloatBits();
        }

        public float Distance(Vector3f other)
        {
            float result = (this.X - other.X) * (this.X - other.X) +
                (this.Y - other.Y) * (this.Y - other.Y) +
                (this.Z - other.Z) * (this.Z - other.Z);
            return (float)Math.Sqrt(result);
        }

        public float DistanceXY(Vector3f other)
        {
            float result = (this.X - other.X) * (this.X - other.X) +
                (this.Y - other.Y) * (this.Y - other.Y);
            return (float)Math.Sqrt(result);
        }

        public bool BitEquals(Vector3f other)
        {
            return    this.X.BitEquals(other.X)
                   && this.Y.BitEquals(other.Y)
                   && this.Z.BitEquals(other.Z);
        }

        public bool BitEqualsXY(Vector3f other)
        {
            return    this.X.BitEquals(other.X)
                   && this.Y.BitEquals(other.Y);
        }

        public override string ToString()
        {
            return this.X + " " + this.Y + " " + this.Z;
        }
    }

    // prepends update count and tick count to every Debug.WriteLine
    public class TimedTraceListener : DefaultTraceListener
    {
        private static TimedTraceListener _instance;
        public static TimedTraceListener Instance
        {
            get { return _instance ?? (_instance = new TimedTraceListener()); }
        }

        private TimedTraceListener() { }

        public int TickCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
        }

        public int UpdateCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
        }

        public override void WriteLine(string message)
        {
            base.WriteLine("SourceSplit: " + this.UpdateCount + " " + this.TickCount + " " + message);
        }
    }
}

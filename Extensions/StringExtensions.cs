using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Extensions
{
    static class StringExtensions
    {

        public static string GetByteString(this uint o)
        {
            return BitConverter.ToString(BitConverter.GetBytes(o)).Replace("-", " ");
        }

        public static string ConvertToHex(this string o)
        {
            string output = "";
            foreach (char i in o)
                output += ((byte)i).ToString("x2") + " ";

            return output;
            //return BitConverter.ToString(Encoding.Default.GetBytes(o)).Replace("-", " ");
        }

        public static string GetByteString(this int o) => GetByteString((uint)o);
        public static string GetByteString(this IntPtr o) => GetByteString((uint)o);
    }
}

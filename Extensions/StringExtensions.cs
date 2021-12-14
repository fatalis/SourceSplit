using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // primitive wildcard search system, currently only supports *
        // examples:    *ee*    matches peek, seek, see
        //              o*      matches orange, origami, and doesn't match house, horse
        //              *o      matches hello, polo, and doesn't match ghost, lone
        public static bool CompareWildcard(this string input, string format)
        {
            if (input == "" || format == "" || !format.Contains('*'))
                return input == format;

            bool first = format[0] != '*';
            bool last = format[format.Count() - 1] != '*';

            List<string> elements = format.Split('*').ToList();
            elements = elements.Where(x => !string.IsNullOrEmpty(x)).ToList();

            while (input != "" && elements.Count() > 0)
            {
                int index = (elements.Count != 1) ? input.IndexOf(elements[0]) : input.LastIndexOf(elements[0]);
                if (index == -1)
                    return false;

                input = input.Substring(index);

                if (elements.Count == 1)
                {
                    if (last && elements[0] != input)
                        return false;
                }

                if (first && index != 0)
                    return false;

                elements.RemoveAt(0);
                first = false;
            }

            return true;
        }
    }
}

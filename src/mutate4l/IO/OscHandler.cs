using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l
{
    public class OscHandler
    {
        public static string GetOscStringKey(string input)
        {
            if (!input.Contains(",s")) return "";
            string key = input.Substring(0, input.IndexOf(",s"));
            return key.TrimEnd('\0');
        }

        public static string GetOscStringValue(string input)
        {
            if (!input.Contains(",s")) return "";
            string value = input.Substring(input.IndexOf(",s") + 4);
            return value.TrimEnd('\0');
        }

        public static Byte[] CreateOscMessage(string route, Int32 arg1, Int32 arg2)
        {
            if (string.IsNullOrEmpty(route)) return new Byte[0];

            route = FourPadString(route) + ",ii\0";
            List<byte> bytes = new List<byte>(route.Length + 8);
            bytes.AddRange(Encoding.ASCII.GetBytes(route));
            bytes.AddRange(Int32ToBytes(arg1));
            bytes.AddRange(Int32ToBytes(arg2));
            return bytes.ToArray();
        }

        public static string FourPadString(string input)
        {
            return input.PadRight(((input.Length / 4) + 1) * 4, '\0');
        }

        public static byte[] Int32ToBytes(Int32 val)
        {
            byte[] result = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian) { Array.Reverse(result); }
            return result;
        }
    }
}

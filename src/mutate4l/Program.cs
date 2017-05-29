using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mutate4l
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient listener = new UdpClient(8008);
            UdpClient sender = new UdpClient();
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 8008);

            try
            {
                byte[] message = CreateOscMessage("/mu4l/clip/get", 1, 3);
                sender.Send(message, message.Length, "localhost", 8009);
                Console.WriteLine("Waiting for broadcast");
                byte[] bytes = listener.Receive(ref groupEP);
                string data = Encoding.ASCII.GetString(bytes);
                Console.WriteLine($"[{Program.GetOscStringKey(data)}] : [{Program.GetOscStringValue(data)}]");

                string[] noteData = Program.GetOscStringValue(data).Split(' ');
                List<Note> notes = new List<Note>(noteData.Length / 4);
                for (var i = 0; i < noteData.Length; i += 4)
                {
                    notes.Add(new Note(byte.Parse(noteData[i]), decimal.Parse(noteData[i + 1]), decimal.Parse(noteData[i + 2]), byte.Parse(noteData[i + 3])));
                }
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }
        }

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

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mutate4l
{
    class Program
    {
        

        static void Main(string[] args)
        {
            bool done = false;

            UdpClient listener = new UdpClient(8008);
            UdpClient sender = new UdpClient();
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 8008);

            try
            {
                sender.Connect("localhost", 8009);
				Byte[] sendBytes = Encoding.ASCII.GetBytes("/p\0\0,s\0\0heisann\0");

				sender.Send(sendBytes, sendBytes.Length);

                while (!done)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);
                    string data = Encoding.ASCII.GetString(bytes);
                    Console.WriteLine($"{Program.GetOscStringKey(data)} : {Program.GetOscStringValue(data)}");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }


            /*
            UdpClient udpClient = new UdpClient();
            try
            {
                udpClient.Connect("localhost", 8008);

                // Sends a message to the host to which you have connected.
                Byte[] sendBytes = Encoding.ASCII.GetBytes("/p\0\0,s\0\0heisann\0");

                udpClient.Send(sendBytes, sendBytes.Length);

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8009);

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine(returnData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }*/
            //CliHandler.Start();
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

    }
}

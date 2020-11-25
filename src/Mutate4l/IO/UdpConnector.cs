using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mutate4l.Core;

namespace Mutate4l.IO
{
    public static class UdpConnector
    {
        public static int ReceivePort = 8022;
        public static int SendPort = 8023;
        private static UdpClient UdpClient;

        public static Clip GetClip(int channel, int clip)
        {
            return GetClipData("/mu4l/clip/get", channel, clip);
        }

        // currently unused
        public static Clip GetSelectedClip()
        {
            return GetClipData("/mu4l/selectedclip/get", 0, 0);
        }

        public static Clip GetClipData(string address, int channel, int clip)
        {
            byte[] message = OscHandler.CreateOscMessage(address, channel, clip);
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "127.0.0.1", SendPort);
                result = udpClient.Receive(ref endPoint);
            }
            var data = Encoding.ASCII.GetString(result);
            var noteData = OscHandler.GetOscStringValue(data);
            return IOUtilities.StringToClip(noteData);
        }

        // currently unused
        public static void SetClips(int trackNo, int startingClipNo, Clip[] clips)
        {
            var i = 0;
            foreach (var clip in clips)
            {
                SetClip(trackNo, startingClipNo + i++, clip);
            }
        }

        // currently unused
        public static void SetClip(int trackNo, int clipNo, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/set", trackNo, clipNo, data);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        public static void SetClipById(string id, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/setbyid", int.Parse(id), 0, data);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        public static void SetClipAsBytesById(byte[] clipData)
        {
            using (var udpClient = new UdpClient())
            {
                udpClient.Send(clipData, clipData.Length, "localhost", SendPort);
                Console.WriteLine($"Sent {clipData.Length} bytes over UDP");
            }
        }

        // currently unused
        public static void SetSelectedClip(Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/selectedclip/set", 0, 0, data);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        public static bool TestCommunication()
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/hello", 0, 0);
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
                result = udpClient.Receive(ref endPoint);
            }
            string data = Encoding.ASCII.GetString(result);
            return data.Contains("/mu4l/out/hello");
        }

        // currently unused
        public static void EnumerateClips()
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/enum", 0, 0);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        public static byte[] WaitForData()
        {
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (UdpClient = new UdpClient(ReceivePort))
            {
                result = UdpClient.Receive(ref endPoint);
            }
            return result;
        }
    }
}

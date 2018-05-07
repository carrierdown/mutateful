using Mutate4l.Dto;
using Mutate4l.Utility;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mutate4l.IO
{
    public static class UdpConnector
    {
        public static int ReceivePort = 8022;
        public static int SendPort = 8023;

        public static Clip GetClip(int channel, int clip)
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/get", channel, clip);
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
                result = udpClient.Receive(ref endPoint);
            }
            var data = Encoding.ASCII.GetString(result);
            var noteData = OscHandler.GetOscStringValue(data);
            return IOUtilities.StringToClip(noteData);
        }

        public static void SetClips(int trackNo, int startingClipNo, Clip[] clips)
        {
            var i = 0;
            foreach (var clip in clips)
            {
                SetClip(trackNo, startingClipNo + i++, clip);
            }
        }

        public static void SetClip(int trackNo, int clipNo, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/set", trackNo, clipNo, data);

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

        public static void EnumerateClips()
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/enum", 0, 0);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }
    }
}

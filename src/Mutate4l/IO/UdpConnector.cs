using Mutate4l.Dto;
using Mutate4l.Utility;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mutate4l.IO
{
    public static class UdpConnector
    {
        public static int ReceivePort = 8022;
        public static int SendPort = 8023;

        public static Clip GetClip(int channel, int clip)
        {
            return GetClipData("/mu4l/clip/get", channel, clip);
        }

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

        public static void SetClipById(string id, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/setbyid", int.Parse(id), 0, data);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

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

        public static void EnumerateClips()
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/enum", 0, 0);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        public static string WaitForData()
        {
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (var udpClient = new UdpClient(ReceivePort))
            {
//                udpClient.Send(message, message.Length, "localhost", SendPort);
                result = udpClient.Receive(ref endPoint);
            }
            string rawData = Encoding.ASCII.GetString(result);
            string data = OscHandler.GetOscStringValue(rawData);
            return data;
        }
    }
}

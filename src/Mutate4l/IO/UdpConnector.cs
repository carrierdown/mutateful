using Mutate4l.Dto;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static UdpClient UdpSender;

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
                udpClient.Send(message, message.Length, "localhost", SendPort);
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

        public static void SetClipAsBytesById(ushort id, Clip clip)
        {
            /*
                Return format:

                2 bytes (id)
                4 bytes (clip length - float)
                1 byte (loop state - 1/0 for on/off)
                2 bytes (number of notes)
                    1 byte  (pitch)
                    4 bytes (start - float)
                    4 bytes (duration - float)
                    1 byte  (velocity)

                Above block repeated N times
            */
            var result = new List<byte>(2 + 4 + 1 + 2 + (10 * clip.Notes.Count));
            result.AddRange(BitConverter.GetBytes(id));
            result.AddRange(BitConverter.GetBytes((Single)clip.Length));
            result.Add((byte)(clip.IsLooping ? 1 : 0));
            result.AddRange(BitConverter.GetBytes((ushort)clip.Notes.Count));

            foreach (var note in clip.Notes)
            {
                result.Add((byte)note.Pitch);
                result.AddRange(BitConverter.GetBytes((Single)note.Start));
                result.AddRange(BitConverter.GetBytes((Single)note.Duration));
                result.Add((byte)note.Velocity);
            }
            using (UdpSender = new UdpClient(ReceivePort))
            {
                UdpSender.Send(result.ToArray(), result.Count, "localhost", SendPort);
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

        public static (List<Clip> Clips, string Formula, ushort Id, byte TrackNo) WaitForData()
        {
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                result = udpClient.Receive(ref endPoint);
            }
            return DecodeData(result);
        }

        public static (List<Clip> Clips, string Formula, ushort Id, byte TrackNo) DecodeData(byte[] data)
        {
            var clips = new List<Clip>();
            ushort id = BitConverter.ToUInt16(data, 0);
            byte trackNo = data[2];
            byte numClips = data[3];
            int dataOffset = 4;

            // Decode clipdata
            while (clips.Count < numClips)
            {
                ClipReference clipReference = new ClipReference(data[dataOffset], data[dataOffset += 1]);
                decimal length = (decimal)BitConverter.ToSingle(data, dataOffset += 1);
                bool isLooping = data[dataOffset += 4] == 1;
                var clip = new Clip(length, isLooping) {
                    ClipReference = clipReference
                };
                ushort numNotes = BitConverter.ToUInt16(data, dataOffset += 1);
                dataOffset += 2;
                for (var i = 0; i < numNotes; i++)
                {
                    clip.Notes.Add(new NoteEvent(
                        data[dataOffset], 
                        (decimal)BitConverter.ToSingle(data, dataOffset += 1), 
                        (decimal)BitConverter.ToSingle(data, dataOffset += 4), 
                        data[dataOffset += 4])
                    );
                    dataOffset++;
                }
                clips.Add(clip);
            }
            // Convert remaining bytes to text containing the formula
            string formula = Encoding.ASCII.GetString(data, dataOffset, data.Length - dataOffset);

            return (clips, formula, id, trackNo);
        }
    }
}

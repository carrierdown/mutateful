using Mutate4l.ClipActions;
using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Utility;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mutate4l.IO
{
    public class UdpConnector
    {
        private UdpClient Listener;
        private UdpClient Sender;
        private IPEndPoint GroupEP;

        public void Open()
        {
            Listener = new UdpClient(8008);
            Sender = new UdpClient();
            GroupEP = new IPEndPoint(IPAddress.Any, 8008);
        }

        public void Close()
        {
            Listener.Close();
            Sender.Close();
        }

        public Clip GetClip(int channel, int clip)
        {
            var notes = new SortedList<Note>();
            string noteData = "";
            try
            {
                byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/get", channel, clip);
                Sender.Send(message, message.Length, "localhost", 8009);
//                Console.WriteLine("Waiting for broadcast");
                byte[] bytes = Listener.Receive(ref GroupEP);
                string data = Encoding.ASCII.GetString(bytes);
//                Console.WriteLine($"[{OscHandler.GetOscStringKey(data)}] : [{OscHandler.GetOscStringValue(data)}]");

                noteData = OscHandler.GetOscStringValue(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return IOUtilities.StringToClip(noteData);
        }

        public void SetClips(int trackNo, int startingClipNo, Clip[] clips)
        {
            var i = 0;
            foreach (var clip in clips)
            {
                SetClip(trackNo, startingClipNo + i++, clip);
            }
        }

        public void SetClip(int trackNo, int clipNo, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/set", trackNo, clipNo, data);
            Sender.Send(message, message.Length, "localhost", 8009);
        }

    }
}

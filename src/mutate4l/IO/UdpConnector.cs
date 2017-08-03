using Mutate4l.Core;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            decimal clipLength = 1;
            bool isLooping = false;
            try
            {
                byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/get", channel, clip);
                Sender.Send(message, message.Length, "localhost", 8009);
//                Console.WriteLine("Waiting for broadcast");
                byte[] bytes = Listener.Receive(ref GroupEP);
                string data = Encoding.ASCII.GetString(bytes);
//                Console.WriteLine($"[{OscHandler.GetOscStringKey(data)}] : [{OscHandler.GetOscStringValue(data)}]");

                string[] noteData = OscHandler.GetOscStringValue(data).Split(' ');
                if (noteData.Length < 2)
                {
                    return null;
                }
                clipLength = decimal.Parse(noteData[0]);
                isLooping = noteData[1] == "1";
                for (var i = 2; i < noteData.Length; i += 4)
                {
                    notes.Add(new Note(byte.Parse(noteData[i]), decimal.Parse(noteData[i + 1], NumberStyles.Float), decimal.Parse(noteData[i + 2], NumberStyles.Float), byte.Parse(noteData[i + 3])));
                }
//                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return new Clip(clipLength, isLooping)
            {
                Notes = notes
            };
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
            string data = $"{clip.Length} {clip.IsLooping}";
            for (var i = 0; i < clip.Notes.Count; i++)
            {
                var note = clip.Notes[i];
                data = string.Join(' ', data, note.Pitch, note.Start.ToString("F4"), note.Duration.ToString("F4"), note.Velocity);
            }
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/set", trackNo, clipNo, data);
            Sender.Send(message, message.Length, "localhost", 8009);
        }

    }
}

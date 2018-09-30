using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mutate4l.IO
{
    public class UdpMultiPacket
    {
        public string this[int i]
        {
            set
            {
                PacketContents[i] = value;
                IsCompleted = PacketContents.All(x => x.Length > 0);
            }
        }
        private string[] PacketContents;
        public int Id { get; }
        public bool IsCompleted { get; private set; } = false;
        public string Content {
            get {
                string result = "";
                PacketContents.ForEach(x => { result += x });
                return result;
            }
        }

        public UdpMultiPacket(int id, int numPackets)
        {
            PacketContents = new string[numPackets];
            Id = id;
        }
    }
}

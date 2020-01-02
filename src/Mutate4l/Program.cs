using Mutate4l.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Mutate4l.Core;

namespace Mutate4l
{
     internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;
            //using var udpSender = new UdpClient();
            using (var udpClient = new UdpClient(8022))
            {
                Console.WriteLine("Welcome to mutateful!");
                Console.WriteLine("Open Ableton Live, drop mutateful-connector.amxd onto one of the tracks, and start entering formulas.");
                // can run something like GetIncomingData, SendOutgoingData, and DoRegularProcessLoop in parallel here
                var queue = Channel.CreateUnbounded<byte[]>();
                
                await Task.WhenAny(
                    DataHandler.ProcessData(udpClient, queue.Writer), 
                    DataHandler.ConsumeQueue(udpClient, queue.Reader), 
                    Task.Run(() =>
                        {
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                        }
                    )
                );
            }
            Console.WriteLine("Exiting... Press any key.");
            Console.ReadKey();
        }
    }
    
    internal static class DataHandler
    {
        private static IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 8023);
        
        private static async IAsyncEnumerable<byte[]> GetUdpDataAsync(UdpClient udpClient)
        {
            while (true)
            {
                UdpReceiveResult result;
                try
                {
                    result = await udpClient.ReceiveAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                yield return result.Buffer;
            }
        }

        public static async Task ProcessData(UdpClient udpClient, ChannelWriter<byte[]> writer)
        {
            await foreach (byte[] values in GetUdpDataAsync(udpClient).ConfigureAwait(false))
            {
                // this should trigger some event that notifies the state of mutateful and possibly triggers a re-evaluation of any formulas (should take into account whether the received data is for a complete clip or just partial)
                Console.WriteLine($"Received datagram of size {values.Length}");
                await writer.WriteAsync(new byte[]
                {
                    104, 2, 0, 0, 128, 64, 1, 13, 0, 60, 0, 0, 0, 0, 0, 0, 128, 62, 100, 62, 0, 0, 128, 62, 0, 0, 128, 62, 100,
                    63, 0, 0, 0, 63, 0, 0, 128, 62, 100, 65, 0, 0, 64, 63, 0, 0, 128, 62, 100, 66, 0, 0, 128, 63, 0, 0, 128, 62,
                    100, 67, 0, 0, 160, 63, 0, 0, 128, 62, 100, 70, 0, 0, 192, 63, 0, 0, 128, 62, 100, 72, 0, 0, 224, 63, 0, 0,
                    128, 62, 100, 60, 0, 0, 48, 64, 0, 0, 128, 62, 100, 62, 0, 0, 64, 64, 0, 0, 128, 62, 100, 63, 0, 0, 80, 64, 0,
                    0, 128, 62, 100, 64, 0, 0, 96, 64, 0, 0, 128, 62, 100, 67, 0, 0, 112, 64, 0, 0, 128, 62, 100
                });
            }
        }
        
        public static async Task ConsumeQueue(UdpClient udpClient, ChannelReader<byte[]> reader)
        {
            await foreach (var item in reader.ReadAllAsync()/*.ConfigureAwait(false)*/)
            {
                Console.WriteLine($"Received data to send, with length {item.Length}");
                udpClient.Send(item, item.Length, EndPoint);
            }
        }
    }
}

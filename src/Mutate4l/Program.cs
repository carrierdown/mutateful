using System;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Mutate4l.IO;
using Mutate4l.State;

namespace Mutate4l
{
     internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            var userInitiatedExit = false;
            
            using (var udpClient = new UdpClient(8022))
            {
                Console.WriteLine("Welcome to mutateful!");
                Console.WriteLine("Open Ableton Live, drop mutateful-connector.amxd onto one of the tracks, and start entering formulas.");
                
                var queue = Channel.CreateUnbounded<InternalCommand>();
                
                await Task.WhenAny(
                    UdpHandler.ProcessUdpDataAsync(udpClient, queue.Writer), 
                    UdpHandler.SendUdpDataAsync(udpClient, queue.Reader), 
                    Task.Run(() =>
                        {
                            Console.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                            userInitiatedExit = true;
                        }
                    )
                );
            }

            if (!userInitiatedExit)
            {
                Console.WriteLine("Mutateful will exit now. Press any key...");
                Console.ReadKey();
            }
        }
    }
}

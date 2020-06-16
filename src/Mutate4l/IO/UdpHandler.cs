using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using Mutate4l.Cli;

namespace Mutate4l.IO
{
    public static class UdpHandler
    {
        private static async IAsyncEnumerable<byte[]> ReceiveUdpDataAsync(UdpClient udpClient)
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

        public static async Task ProcessUdpDataAsync(UdpClient udpClient, ChannelWriter<byte[]> writer, Func<byte[], byte[]> processFunction)
        {
            await foreach (byte[] values in ReceiveUdpDataAsync(udpClient).ConfigureAwait(false))
            {
                // this should trigger some event that notifies the state of mutateful and possibly triggers a re-evaluation of any formulas (should take into account whether the received data is for a complete clip or just partial)
                // Console.WriteLine($"Received datagram of size {values.Length}");
                var result = processFunction(values);
                // var result = CliHandler.HandleData(values);
                if (result.Length > 0)
                    await writer.WriteAsync(result);
            }
        }

        public static async Task SendUdpDataAsync(UdpClient udpClient, ChannelReader<byte[]> reader)
        {
            await foreach (var clipData in reader.ReadAllAsync().ConfigureAwait(false))
            {
                Console.WriteLine($"Received data to send, with length {clipData.Length}");
                try
                {
                    udpClient.Send(clipData, clipData.Length, "127.0.0.1", 8023);
                }
                catch (Exception)
                {
                    Console.WriteLine("Exception occurred while sending UDP data");
                }

                await Task.Delay(10);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.State;
using Mutate4l.Utility;

namespace Mutate4l.IO
{
    public static class UdpHandler
    {
        private const int UdpSendDelay = 10;

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

        public static async Task ProcessUdpDataAsync(UdpClient udpClient, ChannelWriter<InternalCommand> writer, ClipSet clipSet)
        {
            await foreach (byte[] data in ReceiveUdpDataAsync(udpClient).ConfigureAwait(false))
            {
                if (Decoder.IsTypedCommand(data))
                {
                    Decoder.HandleTypedCommand(data, clipSet, writer);
                }
                else // old logic
                {
                    var result = CliHandler.HandleInput(data);

                    if (result != LegacyClipSlot.Empty)
                    {
                        var clipData = IOUtilities.GetClipAsBytes(result.Id, result.Clip).ToArray();
                        Console.WriteLine($"Received data to send, with length {clipData.Length}");
                        try
                        {
                            await udpClient.SendAsync(clipData, clipData.Length, "127.0.0.1", 8023);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Exception occurred while sending UDP data");
                        }
                    }
                }
            }
        }

        public static async Task SendUdpDataAsync(UdpClient udpClient, ChannelReader<InternalCommand> reader)
        {
            await foreach (var internalCommand in reader.ReadAllAsync().ConfigureAwait(false))
            {
                switch (internalCommand.Type)
                {
                    case InternalCommandType.SetClipDataOnClient:
                    case InternalCommandType.SetClipDataOnClientLive11:
                        Console.WriteLine($"Got clip at [{internalCommand.ClipSlot.ClipReference}] - passing along to client(s)...");
                        var clipData = internalCommand.Type == InternalCommandType.SetClipDataOnClient ? 
                            IOUtilities.GetClipAsBytesV2(internalCommand.ClipSlot.Clip).ToArray() : 
                            IOUtilities.GetClipAsBytesLive11(internalCommand.ClipSlot.Clip).ToArray();
                        try
                        {
                            await udpClient.SendAsync(clipData, clipData.Length, "127.0.0.1", 8023);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Exception occurred while sending UDP data");
                        }
                        break;                    
                }
                await Task.Delay(UdpSendDelay);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using Mutate4l.Cli;
using Mutate4l.State;
using Mutate4l.Utility;

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

        public static async Task ProcessUdpDataAsync(UdpClient udpClient, ChannelWriter<InternalCommand> writer)
        {
            await foreach (byte[] data in ReceiveUdpDataAsync(udpClient).ConfigureAwait(false))
            {
                if (Decoder.IsTypedCommand(data))
                {
                    // new logic for handling input
                    switch (Decoder.GetCommandType(data[3]))
                    {
                        case InternalCommandType.OutputString:
                            string text = Decoder.GetText(data);
                            Console.WriteLine(text);
                            break;
                        case InternalCommandType.SetClipSlot:
                            break;
                        case InternalCommandType.SetAndEvaluateClipSlot:
                            break;
                        case InternalCommandType.EvaluateClipSlots:
                            break;
                        case InternalCommandType.UnknownCommand:
                            break;
                    }
                }
                else // old logic
                {
                    var result = CliHandler.HandleInput(data);
                    if (result != ClipSlot.Empty)
                        await writer.WriteAsync(new InternalCommand(InternalCommandType.SetClipSlot, result, new[] { result.Clip.ClipReference }));
                }
            }
        }

        public static async Task SendUdpDataAsync(UdpClient udpClient, ChannelReader<InternalCommand> reader)
        {
            await foreach (var internalCommand in reader.ReadAllAsync().ConfigureAwait(false))
            {
                var clipData = IOUtilities.GetClipAsBytes(internalCommand.ClipSlot.Id, internalCommand.ClipSlot.Clip).ToArray();
                Console.WriteLine($"Received data to send, with length {clipData.Length}");
                try
                {
                    await udpClient.SendAsync(clipData, clipData.Length, "127.0.0.1", 8023);
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
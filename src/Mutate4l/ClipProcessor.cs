using Mutate4l.Cli;
using Mutate4l.Commands;
using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.IO;
using System.Linq;

namespace Mutate4l
{
    public static class ClipProcessor
    {
        public static Result ProcessChainedCommand(ChainedCommand chainedCommand)
        {
            Clip[] sourceClips = chainedCommand.SourceClips.Where(c => c.Notes.Count > 0).ToArray();
            if (sourceClips.Length < 1)
            {
                return new Result("No clips or empty clips specified. Aborting.");
            }

            Clip[] currentSourceClips = sourceClips;
            ProcessResultArray<Clip> resultContainer = new ProcessResultArray<Clip>("No commands specified");
            foreach (var command in chainedCommand.Commands)
            {
                resultContainer = ProcessCommand(command, currentSourceClips, chainedCommand.TargetMetaData);
                if (resultContainer.Success)
                    currentSourceClips = resultContainer.Result;
                else
                    break;
            }
            if (resultContainer.Success && resultContainer.Result.Length > 0)
            {
                UdpConnector.SetClipAsBytesById(chainedCommand.TargetMetaData.Id, resultContainer.Result[0]);
            }
            else
                return new Result(resultContainer.ErrorMessage, "Error applying formula");

            return new Result(resultContainer.Success, resultContainer.ErrorMessage);
        }

        public static ProcessResultArray<Clip> ProcessCommand(Command command, Clip[] clips, ClipMetaData targetMetadata)
        {
            ProcessResultArray<Clip> resultContainer;
            switch (command.Id)
            {
                case TokenType.Arpeggiate:
                    resultContainer = Arpeggiate.Apply(command, clips);
                    break;
                case TokenType.Concat:
                    resultContainer = Concat.Apply(clips);
                    break;
                case TokenType.Constrain:
                    resultContainer = Constrain.Apply(command, clips);
                    break;
                case TokenType.Filter:
                    resultContainer = Filter.Apply(command, clips);
                    break;
                case TokenType.Interleave:
                    resultContainer = Interleave.Apply(command, targetMetadata, clips); 
                    break;
/*                case TokenType.InterleaveEvent:
                    var options = OptionParser.ParseOptions<InterleaveOptions>(command);
                    options.Mode = InterleaveMode.Event;
                    resultContainer = Interleave.Apply(options, targetMetadata, clips);
                    break;*/
                case TokenType.Monophonize:
                    resultContainer = Monophonize.Apply(clips);
                    break;
                case TokenType.Ratchet:
                    resultContainer = Ratchet.Apply(command, clips);
                    break;
                case TokenType.Relength:
                    resultContainer = Relength.Apply(command, clips);
                    break;
                case TokenType.Resize:
                    resultContainer = Resize.Apply(command, clips);
                    break;
                case TokenType.Scan:
                    resultContainer = Scan.Apply(command, clips);
                    break;
                case TokenType.Shuffle:
                    resultContainer = Shuffle.Apply(command, clips);
                    break;
                case TokenType.Slice:
                    resultContainer = Slice.Apply(command, clips);
                    break;
                case TokenType.Take:
                    resultContainer = Take.Apply(command, clips);
                    break;
                case TokenType.Transpose:
                    resultContainer = Transpose.Apply(command, clips);
                    break;
                default:
                    // todo: error here
                    return new ProcessResultArray<Clip>($"Unsupported command {command.Id}");
            }
            return resultContainer;
        }
    }
}

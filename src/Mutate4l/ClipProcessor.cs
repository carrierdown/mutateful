using Mutate4l.Cli;
using Mutate4l.Commands;
using Mutate4l.Core;
using Mutate4l.IO;
using System.Linq;
using Mutate4l.Commands.Experimental;
using Mutate4l.Utility;

namespace Mutate4l
{
    public static class ClipProcessor
    {
        public static ProcessResultArray<Clip> ProcessChainedCommand(ChainedCommand chainedCommand)
        {
            Clip[] sourceClips = chainedCommand.SourceClips.Where(c => c.Notes.Count > 0).ToArray();
            if (sourceClips.Length < 1)
            {
                return new ProcessResultArray<Clip>("No clips or empty clips specified. Aborting.");
            }

            var currentSourceClips = sourceClips;
            var resultContainer = new ProcessResultArray<Clip>("No commands specified");
            foreach (var command in chainedCommand.Commands)
            {
                resultContainer = ProcessCommand(command, currentSourceClips, chainedCommand.TargetMetaData);
                if (resultContainer.Success)
                    currentSourceClips = resultContainer.Result;
                else
                    break;
            }
            return resultContainer;
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
                case TokenType.InterleaveEvent:
                    var (success, msg) = OptionParser.TryParseOptions(command, out InterleaveOptions options);
                    if (!success)
                    {
                        return new ProcessResultArray<Clip>(msg);
                    }
                    options.Mode = InterleaveMode.Event;
                    resultContainer = Interleave.Apply(options, targetMetadata, clips);
                    break;
                case TokenType.Mask:
                    resultContainer = Mask.Apply(command, clips);
                    break;
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
                case TokenType.SetLength:
                    resultContainer = SetLength.Apply(command, clips);
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

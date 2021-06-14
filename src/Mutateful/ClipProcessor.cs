using System.Collections.Generic;
using System.Linq;
using Mutateful.Commands;
using Mutateful.Commands.Experimental;
using Mutateful.Compiler;
using Mutateful.Core;

namespace Mutate4l
{
    public static class ClipProcessor
    {
        public static ProcessResultArray<Clip> ProcessCommand(Command command, Clip[] incomingClips, ClipMetaData targetMetadata)
        {
            var clips = new Clip[incomingClips.Length];
            for (var i = 0; i < incomingClips.Length; i++)
            {
                clips[i] = new Clip(incomingClips[i]);
            }

            return command.Id switch
            {
                TokenType.Arpeggiate => Arpeggiate.Apply(command, clips),
                TokenType.Concat => Concat.Apply(clips),
                TokenType.Crop => Crop.Apply(command, clips),
                TokenType.Filter => Filter.Apply(command, clips),
                TokenType.Invert => Invert.Apply(command, clips),
                TokenType.Interleave => Interleave.Apply(command, targetMetadata, clips, InterleaveMode.NotSpecified),
                TokenType.InterleaveEvent => Interleave.Apply(command, targetMetadata, clips, InterleaveMode.Event),
                TokenType.Legato => Legato.Apply(clips),
                TokenType.Loop => Loop.Apply(command, clips),
                TokenType.Mask => Mask.Apply(command, clips),
                TokenType.Monophonize => Monophonize.Apply(clips),
                TokenType.Padding => Padding.Apply(command, clips),
                TokenType.Quantize => Quantize.Apply(command, clips),
                TokenType.Ratchet => Ratchet.Apply(command, clips),
                TokenType.Relength => Relength.Apply(command, clips),
                TokenType.Remap => Remap.Apply(command, clips),
                TokenType.Resize => Resize.Apply(command, clips),
                TokenType.Scale => Scale.Apply(command, clips),
                TokenType.Scan => Scan.Apply(command, clips),
                TokenType.SetLength => SetLength.Apply(command, clips),
                TokenType.SetPitch => SetPitch.Apply(command, clips),
                TokenType.SetRhythm => SetRhythm.Apply(command, clips),
                TokenType.Shuffle => Shuffle.Apply(command, clips),
                TokenType.Skip => Skip.Apply(command, clips),
                TokenType.Slice => Slice.Apply(command, clips),
                TokenType.Take => Take.Apply(command, clips),
                TokenType.Transpose => Transpose.Apply(command, clips),
                TokenType.VelocityScale => VelocityScale.Apply(command, clips),
                _ => new ProcessResultArray<Clip>($"Unsupported command {command.Id}")
            };
        }

        public static ProcessResultArray<Clip> ProcessChainedCommand(ChainedCommand chainedCommand)
        {
            Clip[] sourceClips = chainedCommand.SourceClips.Where(c => c.Notes.Count > 0).ToArray();
            if (sourceClips.Length < 1)
            {
                return new ProcessResultArray<Clip>("No clips or empty clips specified. Aborting.");
            }

            var currentSourceClips = sourceClips;
            var resultContainer = new ProcessResultArray<Clip>("No commands specified");
            var warnings = new List<string>();
            foreach (var command in chainedCommand.Commands)
            {
                resultContainer = ProcessCommand(command, currentSourceClips, chainedCommand.TargetMetaData);
                if (resultContainer.Success)
                {
                    currentSourceClips = resultContainer.Result;
                    if (resultContainer.WarningMessage.Length > 0)
                    {
                        warnings.Add(resultContainer.WarningMessage);
                    }
                }
                else
                {
                    break;
                }
            }

            if (warnings.Count > 0)
            {
                resultContainer = new ProcessResultArray<Clip>(resultContainer.Success, resultContainer.Result, resultContainer.ErrorMessage, string.Join(System.Environment.NewLine, warnings));
            }
            return resultContainer;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.State;
using static Mutate4l.State.InternalCommandType;

namespace Mutate4l.IO
{
    public static class Decoder
    {
        public const byte TypedDataFirstByte = 127;
        public const byte TypedDataSecondByte = 126;
        public const byte TypedDataThirdByte = 125;

        public const byte StringDataSignifier = 124;
        public const byte SetClipDataSignifier = 255;
        public const byte SetFormulaSignifier = 254;
        public const byte EvaluateFormulasSignifier = 253;

        public static bool IsStringData(byte[] result)
        {
            return result.Length > 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte && result[3] == StringDataSignifier;
        }

        public static bool IsTypedCommand(byte[] result)
        {
            return result.Length >= 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte;
        }

        public static InternalCommandType GetCommandType(byte dataSignifier)
        {
            return dataSignifier switch
            {
                StringDataSignifier => ServerOutputString,
                SetClipDataSignifier => ServerSetClipData,
                SetFormulaSignifier => ServerSetFormula,
                EvaluateFormulasSignifier => EvaluateFormulas,
                _ => UnknownCommand
            };
        }

        public static (int trackNo, int clipNo, string formula) GetFormula(byte[] data)
        {
            int trackNo = data[0];
            int clipNo = data[1];
            var formula = Encoding.UTF8.GetString(data[2..]);
            return (trackNo, clipNo, formula);
        }
        
        public static string GetText(byte[] data)
        {
            if (data.Length < 5) return "";
            return Encoding.UTF8.GetString(data[4..]);
        }
        
        public static void HandleTypedCommand(byte[] data, ClipSet clipSet, ChannelWriter<InternalCommand> writer)
        {
            switch (GetCommandType(data[3]))
            {
                case ServerOutputString:
                    var text = Decoder.GetText(data);
                    Console.WriteLine(text);
                    break;
                case ServerSetFormula:
                    var (trackNo, clipNo, formula) = GetFormula(data[4..]);
                    
                    var parsedFormula = Parser.ParseFormula(formula);
                    if (parsedFormula.Success)
                    {
                        Console.WriteLine($"{trackNo}, {clipNo}: Incoming formula {formula}");
                        var clipRef = new ClipReference(trackNo, clipNo);
                        var clipSlot = new ClipSlot(formula, new Clip(clipRef), parsedFormula.Result);
                        clipSet[clipSlot.ClipReference] = clipSlot;
                    }
                    break;
                case ServerSetClipData:
                    var clip = Decoder.GetSingleClip(data[4..]);
                    Console.WriteLine($"{clip.ClipReference.Track}, {clip.ClipReference.Clip} Incoming clip data");
                    if (clip != Clip.Empty)
                    {
                        var clipSlot = new ClipSlot("", clip, Formula.Empty);
                        clipSet[clipSlot.ClipReference] = clipSlot;
                    }
                    break;
                case EvaluateFormulas:
                    // - check that all ClipReferences resolve to a ClipSlot containing either formula or clipdata
                    // - check for circular dependencies by visiting each clipslot referenced by each formula and making sure the starting clipslot is never visited again
                    // - create dependency graph for formulas by maintaining a list of each clipslot and the clips they depend on
                    // - walk dependency graph, processing each formula and populating referenced clips first.
                    // - after each formula has finished processing, send resulting clip as InternalCommand of type SetClipData to ChannelWriter
                    var (successfulClips, failedClips) = DoEvaluateFormulas(clipSet);
                    var warningMessage = "";
                    if (failedClips.Count > 0)
                    {
                        warningMessage = $"Errors encountered while processing formulas at locations {string.Join(", ", failedClips)}";
                    }
                    foreach (var clipRef in successfulClips)
                    {
                        writer.WriteAsync(new InternalCommand(ClientSetClipData, clipSet[clipRef]));
                    }
                    // - note: maybe use different id's for outgoing and incoming commands
                    break;
                case UnknownCommand:
                    break;
            }
        }
        
        // todo: refactor - move somewhere else
        public static (List<ClipReference> successfulClips, List<ClipReference> failedClips) DoEvaluateFormulas(ClipSet clipSet)
        {
            var successfulClips = new List<ClipReference>();
            var failedClips = new List<ClipReference>();

            if (clipSet.AllReferencedClipsValid()/* && !clipSet.HasCircularDependencies()*/)
            {
                var orderedClipRefs = clipSet.GetClipReferencesInProcessableOrder();
                if (orderedClipRefs.Success)
                {
                    Console.WriteLine($"Clips to process: {string.Join(", ", orderedClipRefs.Result.Select(x => x.ToString()))}");
                    var clipsToProcess = orderedClipRefs.Result.Select(x => clipSet[x]);

                    foreach (var clip in clipsToProcess)
                    {
                        foreach (var referencedClip in clip.Formula.AllReferencedClips)
                        {
                            clip.Formula.ClipSlotsByClipReference.Add(referencedClip, clipSet[referencedClip]);
                        }

                        var formula = clip.Formula;
                        var flattenedTokenList = formula.Commands.SelectMany(c => c.Options.Values.SelectMany(o => o))
                            .Concat(formula.Commands.SelectMany(x => x.DefaultOptionValues));
                        
                        // todo: error handling
                        
                        foreach (var token in flattenedTokenList.Where(t => t.IsClipReference))
                        {
                            token.Clip = clip.Formula.ClipSlotsByClipReference[ClipReference.FromString(token.Value)].Clip;
                        }
                        
                        var processedCommand = ClipProcessor.ProcessChainedCommand(new ChainedCommand(
                            formula.Commands, 
                            formula.SourceClipReferences.Select(x => formula.ClipSlotsByClipReference[x].Clip).ToArray(), 
                            new ClipMetaData(0, (byte) clip.ClipReference.Track))
                        );
                        if (processedCommand.Success)
                        {
                            successfulClips.Add(clip.ClipReference);
                            var processedClip = processedCommand.Result[0];
                            processedClip.ClipReference = clip.ClipReference;
                            clipSet[clip.ClipReference].Clip = processedClip;
                        }
                        else
                        {
                            failedClips.Add(clip.ClipReference);
                        } 
                    }
                }
            }
            return (successfulClips, failedClips);
        }

        public static Clip GetSingleClip(byte[] data)
        {
            var offset = 0;
            var clipReference = new ClipReference(data[offset], data[offset += 1]);
            decimal length = (decimal)BitConverter.ToSingle(data, offset += 1);
            bool isLooping = data[offset += 4] == 1;
            var clip = new Clip(length, isLooping)
            {
                ClipReference = clipReference
            };
            ushort numNotes = BitConverter.ToUInt16(data, offset += 1);
            offset += 2;
            for (var i = 0; i < numNotes; i++)
            {
                clip.Notes.Add(new NoteEvent(
                    data[offset], 
                    (decimal)BitConverter.ToSingle(data, offset += 1), 
                    (decimal)BitConverter.ToSingle(data, offset += 4), 
                    data[offset += 4])
                );
                offset++;
            }
            return clip;
        }
        
        public static (List<Clip> Clips, string Formula, ushort Id, byte TrackNo) DecodeData(byte[] data)
        {
            var clips = new List<Clip>();
            ushort id = BitConverter.ToUInt16(data, 0);
            byte trackNo = data[2];
            byte numClips = data[3];
            int dataOffset = 4;

            // Decode clipdata
            while (clips.Count < numClips)
            {
                ClipReference clipReference = new ClipReference(data[dataOffset], data[dataOffset += 1]);
                decimal length = (decimal)BitConverter.ToSingle(data, dataOffset += 1);
                bool isLooping = data[dataOffset += 4] == 1;
                var clip = new Clip(length, isLooping) {
                    ClipReference = clipReference
                };
                ushort numNotes = BitConverter.ToUInt16(data, dataOffset += 1);
                dataOffset += 2;
                for (var i = 0; i < numNotes; i++)
                {
                    clip.Notes.Add(new NoteEvent(
                        data[dataOffset], 
                        (decimal)BitConverter.ToSingle(data, dataOffset += 1), 
                        (decimal)BitConverter.ToSingle(data, dataOffset += 4), 
                        data[dataOffset += 4])
                    );
                    dataOffset++;
                }
                clips.Add(clip);
            }
            // Convert remaining bytes to text containing the formula
            string formula = Encoding.ASCII.GetString(data, dataOffset, data.Length - dataOffset);

            return (clips, formula, id, trackNo);
        }
    }
}
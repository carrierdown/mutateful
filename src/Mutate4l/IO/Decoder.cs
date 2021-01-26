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
        private const byte TypedDataFirstByte = 127;
        private const byte TypedDataSecondByte = 126;
        private const byte TypedDataThirdByte = 125;

        private const byte StringDataSignifier = 124;
        private const byte SetClipDataSignifier = 255;
        private const byte SetFormulaSignifier = 254;
        private const byte EvaluateFormulasSignifier = 253;

        public static bool IsStringData(byte[] result)
        {
            return result.Length > 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte && result[3] == StringDataSignifier;
        }

        public static bool IsTypedCommand(byte[] result)
        {
            return result.Length > 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte;
        }

        public static InternalCommandType GetCommandType(byte dataSignifier)
        {
            return dataSignifier switch
            {
                StringDataSignifier => OutputString,
                SetClipDataSignifier => MutatefulSetClipData,
                SetFormulaSignifier => MutatefulSetFormula,
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
                case OutputString:
                    var text = Decoder.GetText(data);
                    Console.WriteLine(text);
                    break;
                case MutatefulSetFormula:
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
                case MutatefulSetClipData:
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
                        warningMessage = string.Join(',', failedClips.Select(x => x.ClipReference));
                    }
                    foreach (var resultClip in successfulClips)
                    {
                        clipSet[resultClip.ClipReference].Clip = resultClip;
                        writer.WriteAsync(new InternalCommand(LiveSetClipData, clipSet[resultClip.ClipReference]));
                    }
                    // - note: maybe use different id's for outgoing and incoming commands
                    break;
                case UnknownCommand:
                    break;
            }
        }
        
        // todo: refactor - move somewhere else
        public static (List<Clip> successfulClips, List<Clip> failedClips) DoEvaluateFormulas(ClipSet clipSet)
        {
            var successfulClips = new List<Clip>();
            var failedClips = new List<Clip>();

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
                            successfulClips.Add(processedCommand.Result[0]);
                        }
                        else
                        {
                            failedClips.Add(new Clip(clip.ClipReference));
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
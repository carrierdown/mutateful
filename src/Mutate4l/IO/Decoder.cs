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
                SetClipDataSignifier => SetClipData,
                SetFormulaSignifier => SetFormula,
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
                case SetFormula:
                    var (trackNo, clipNo, formula) = Decoder.GetFormula(data[4..]);
                    Console.WriteLine($"Incoming formula {formula}");
                    
                    var parsedFormula = Parser.ParseFormula(formula);
                    if (parsedFormula.Success)
                    {
                        var clipRef = new ClipReference(trackNo, clipNo);
                        var clipSlot = new ClipSlot(formula, new Clip(clipRef), parsedFormula.Result);
                        clipSet.ApplyInternalCommand(new InternalCommand(SetClipData, clipSlot));
                    }
                    break;
                case SetClipData:
                    Console.WriteLine("Incoming clip data");
                    var clip = Decoder.GetSingleClip(data[4..]);
                    if (clip != Clip.Empty)
                    {
                        var clipSlot = new ClipSlot("", clip, Formula.Empty);
                        var internalCommand = new InternalCommand(SetClipData, clipSlot);
                        clipSet.ApplyInternalCommand(internalCommand);
                    }
                    break;
                case EvaluateFormulas:
                    // - check that all ClipReferences resolve to a ClipSlot containing either formula or clipdata
                    // - check for circular dependencies by visiting each clipslot referenced by each formula and making sure the starting clipslot is never visited again
                    if (clipSet.AllReferencedClipsValid()/* && !clipSet.HasCircularDependencies()*/)
                    {
                        var clipsToProcess = clipSet.GetClipReferencesInProcessableOrder();
                        Console.WriteLine($"Clips to process: {string.Join(", ", clipsToProcess.Result.Select(x => x.ToString()))}");
                        
                    }
                    // - create dependency graph for formulas by maintaining a list of each clipslot and the clips they depend on
                    // - walk dependency graph, processing each formula and populating referenced clips first.
                    // - after each formula has finished processing, send resulting clip as InternalCommand of type SetClipData to ChannelWriter
                    
                    // - note: maybe use different id's for outgoing and incoming commands
                    break;
                case UnknownCommand:
                    break;
            }
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
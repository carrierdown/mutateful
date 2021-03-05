using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.State;
using static Mutate4l.State.InternalCommandType;

namespace Mutate4l.IO
{
    public static class CommandHandler
    {
        public static void OutputString(byte[] data)
        {
            var text = Decoder.GetText(data);
            Console.WriteLine(text);
        }

        public static void SetFormulaOnServer(byte[] data, ClipSet clipSet)
        {
            var (trackNo, clipNo, formula) = Decoder.GetFormula(data[4..]);
                    
            var parsedFormula = Parser.ParseFormula(formula);
            if (parsedFormula.Success)
            {
                Console.WriteLine($"{trackNo}, {clipNo}: Incoming formula {formula}");
                var clipRef = new ClipReference(trackNo, clipNo);
                var clipSlot = new ClipSlot(formula, new Clip(clipRef), parsedFormula.Result);
                clipSet[clipSlot.ClipReference] = clipSlot;
            }
        }

        public static void SetClipDataOnServer(byte[] data, ClipSet clipSet)
        {
            var clip = Decoder.GetSingleClip(data[4..]);
            Console.WriteLine($"{clip.ClipReference.Track}, {clip.ClipReference.Clip} Incoming clip data");
            if (clip != Clip.Empty)
            {
                var clipSlot = new ClipSlot("", clip, Formula.Empty);
                clipSet[clipSlot.ClipReference] = clipSlot;
            }
        }

        public static void EvaluateFormulas(ClipSet clipSet, ChannelWriter<InternalCommand> writer)
        {
            if (!clipSet.AllReferencedClipsValid()) return;
            var orderedClipRefs = clipSet.GetClipReferencesInProcessableOrder();
            if (!orderedClipRefs.Success) return;
            Console.WriteLine($"Clips to process: {string.Join(", ", orderedClipRefs.Result.Select(x => x.ToString()))}");

            var clipsToProcess = clipSet.GetClipSlotsFromClipReferences(orderedClipRefs.Result);
            var (successfulClips, failedClips) = clipSet.ProcessClips(clipsToProcess);
                    
            if (failedClips.Count > 0)
            {
                Console.WriteLine($"Errors encountered while processing formulas at locations {string.Join(", ", failedClips)}");
            }
            foreach (var clipRef in successfulClips)
            {
                writer.WriteAsync(new InternalCommand(SetClipDataOnClient, clipSet[clipRef]));
            }
        }

        public static void SetAndEvaluateClipDataOnServer(byte[] data, ClipSet clipSet, ChannelWriter<InternalCommand> writer)
        {
            var clipToEvaluate = Decoder.GetSingleClip(data[4..]);
            Console.WriteLine(
                $"{clipToEvaluate.ClipReference.Track}, {clipToEvaluate.ClipReference.Clip} Incoming clip data to evaluate");
            if (clipToEvaluate != Clip.Empty)
            {
                var clipSlot = new ClipSlot("", clipToEvaluate, Formula.Empty);
                clipSet[clipSlot.ClipReference] = clipSlot;
                var clipReferences = clipSet.GetAllDependentClipRefsFromClipRef(clipSlot.ClipReference);
                var allClipsByClipRef = clipSet.GetAllReferencedClipsByClipRef();
                var orderedClipReferences = clipSet.GetClipReferencesInProcessableOrder(
                    clipReferences.Distinct().ToDictionary(
                        key => key,
                        key => allClipsByClipRef[key]
                            .Where(x => clipReferences.Contains(x))
                            .ToList()));

                Console.WriteLine($"Found dependencies: {string.Join(' ', clipReferences.Select(x => x.ToString()))}");
                Console.WriteLine($"Found sorted: {string.Join(' ', orderedClipReferences.Result.Select(x => x.ToString()))}");
                
                var clipsToProcess = clipSet.GetClipSlotsFromClipReferences(orderedClipReferences.Result);
                var (successfulClips, failedClips) = clipSet.ProcessClips(clipsToProcess);
                    
                if (failedClips.Count > 0)
                {
                    Console.WriteLine($"Errors encountered while processing formulas at locations {string.Join(", ", failedClips)}");
                }
                foreach (var clipRef in successfulClips)
                {
                    writer.WriteAsync(new InternalCommand(SetClipDataOnClient, clipSet[clipRef]));
                }
            }
        }        
        
        public static void SetAndEvaluateFormulaOnServer(byte[] data, ClipSet clipSet, ChannelWriter<InternalCommand> writer)
        {
            var (trackNo, clipNo, formula) = Decoder.GetFormula(data[4..]);
            Console.WriteLine($"{trackNo}, {clipNo} Incoming formula to evaluate");
            
            var parsedFormula = Parser.ParseFormula(formula);
            if (parsedFormula.Success)
            {
                Console.WriteLine($"{trackNo}, {clipNo}: Incoming formula {formula}");
                var clipRef = new ClipReference(trackNo, clipNo);
                var clipSlot = new ClipSlot(formula, new Clip(clipRef), parsedFormula.Result);
                clipSet[clipSlot.ClipReference] = clipSlot;
                var (successfulClips, failedClips) = clipSet.ProcessClips(new [] {clipSlot});
                foreach (var clip in successfulClips)
                {
                    writer.WriteAsync(new InternalCommand(SetClipDataOnClient, clipSet[clip]));
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mutateful.Compiler;
using Mutateful.Core;
using Mutateful.State;

namespace Mutateful.IO
{
    public class CommandHandler
    {
        private readonly ClipSet ClipSet;
        
        public CommandHandler()
        {
            ClipSet = new ClipSet();
        }

        public void SetFormula(int trackNo, int clipNo, string formula)
        {
            var parsedFormula = Parser.ParseFormula(formula);
            if (parsedFormula.Success)
            {
                var clipRef = new ClipReference(trackNo, clipNo);
                var clipSlot = new ClipSlot(formula, new Clip(clipRef), parsedFormula.Result);
                ClipSet[clipSlot.ClipReference] = clipSlot;
            }
        }

        public void SetClipData(Clip clip)
        {
            if (clip != Clip.Empty)
            {
                var clipSlot = new ClipSlot("", clip, Formula.Empty);
                ClipSet[clipSlot.ClipReference] = clipSlot;
            }
        }

        public (List<Clip> successfulClips, List<string> errors) EvaluateFormulas()
        {
            if (!ClipSet.AllReferencedClipsValid()) return (new List<Clip>(), new List<string>());

            var orderedClipRefs = ClipSet.GetClipReferencesInProcessableOrder();
            if (!orderedClipRefs.Success) return (new List<Clip>(), new List<string>());
            Console.WriteLine($"Clips to process: {string.Join(", ", orderedClipRefs.Result.Select(x => x.ToString()))}");

            var clipsToProcess = ClipSet.GetClipSlotsFromClipReferences(orderedClipRefs.Result);
            var (successfulClips, errors) = ClipSet.ProcessClips(clipsToProcess);
            return (ClipSet.GetClipsFromClipReferences(successfulClips).ToList(), errors);
        }

        public (List<Clip> successfulClips, List<string> errors) SetAndEvaluateClipData(Clip clipToEvaluate)
        {
            if (clipToEvaluate == Clip.Empty) return (new List<Clip>(), new List<string> {"Nothing to evaluate"});

            var clipSlot = new ClipSlot("", clipToEvaluate, Formula.Empty);
            ClipSet[clipSlot.ClipReference] = clipSlot;
            var clipReferences = ClipSet.GetAllDependentClipRefsFromClipRef(clipSlot.ClipReference);
            var allClipsByClipRef = ClipSet.GetAllReferencedClipsByClipRef();
            var orderedClipReferences = ClipSet.GetClipReferencesInProcessableOrder(
                clipReferences.Distinct().ToDictionary(
                    key => key,
                    key => allClipsByClipRef[key]
                        .Where(x => clipReferences.Contains(x))
                        .ToList()));

            Debug.WriteLine($"Found dependencies: {string.Join(' ', clipReferences.Select(x => x.ToString()))}");
            Debug.WriteLine($"Found sorted: {string.Join(' ', orderedClipReferences.Result.Select(x => x.ToString()))}");
            
            var clipsToProcess = ClipSet.GetClipSlotsFromClipReferences(orderedClipReferences.Result);
            var (successfulClips, errors) = ClipSet.ProcessClips(clipsToProcess);

            return (ClipSet.GetClipsFromClipReferences(successfulClips).ToList(), errors);
        }        
        
        public (List<Clip> successfulClips, List<string> errors) SetAndEvaluateFormula(string formula, int trackNo, int clipNo)
        {
            var parsedFormula = Parser.ParseFormula(formula);
            if (!parsedFormula.Success) return (new List<Clip>(), new List<string>());

            var clipRef = new ClipReference(trackNo, clipNo);
            var clipSlot = new ClipSlot(formula, new Clip(clipRef), parsedFormula.Result);
            ClipSet[clipSlot.ClipReference] = clipSlot;
            var (successfulClipRefs, errors) = ClipSet.ProcessClips(new [] {clipSlot});
            return (ClipSet.GetClipsFromClipReferences(successfulClipRefs).ToList(), errors);
        }
    }
}
using System.Diagnostics;

namespace Mutateful.IO;

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

    public CommandHandlerResult EvaluateFormulas()
    {
        if (!ClipSet.AllReferencedClipsValid()) return CommandHandlerResult.AbortedResult with {Errors = new List<string> {"Not all referenced clips are valid - aborting evaluation of formulas."}};

        var orderedClipRefs = ClipSet.GetClipReferencesInProcessableOrder();
        if (!orderedClipRefs.Success) return CommandHandlerResult.AbortedResult with {Errors = new List<string> {"Unable to fetch clip references in processable order - aborting evaluation of formulas."}};
        Console.WriteLine($"Clips to process: {string.Join(", ", orderedClipRefs.Result.Select(x => x.ToString()))}");

        var clipsToProcess = ClipSet.GetClipSlotsFromClipReferences(orderedClipRefs.Result);
        var (successfulClips, errors) = ClipSet.ProcessClips(clipsToProcess);
        return CommandHandlerResult.CompletedResult with { SuccessfulClips = ClipSet.GetClipsFromClipReferences(successfulClips).ToList(), Errors = errors };
    }

    public CommandHandlerResult SetAndEvaluateClipData(Clip clipToEvaluate)
    {
        if (ClipSet[clipToEvaluate.ClipReference] != ClipSlot.Empty && clipToEvaluate.Equals(ClipSet[clipToEvaluate.ClipReference].Clip)) 
            return CommandHandlerResult.AbortedResult with {Warnings = new List<string> {$"Aborted evaluation of clip at {clipToEvaluate.ClipReference.ToString()} since it was unchanged."}};
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

        return CommandHandlerResult.CompletedResult with { SuccessfulClips = ClipSet.GetClipsFromClipReferences(successfulClips).ToList(), Errors = errors };
    }        
    
    public CommandHandlerResult SetAndEvaluateFormula(string formula, int trackNo, int clipNo)
    {
        var clipRef = new ClipReference(trackNo, clipNo);
        if (ClipSet[clipRef] != ClipSlot.Empty && ClipSet[clipRef].Name == formula) return CommandHandlerResult.AbortedResult with {Warnings = new List<string> {$"Aborted evaluation of formula at {clipRef.ToString()} since it was unchanged."}};
        var (success, result, errorMessage) = Parser.ParseFormula(formula);
        if (!success) return CommandHandlerResult.AbortedResult with { Errors = new List<string> { errorMessage } };

        var clipSlot = new ClipSlot(formula, new Clip(clipRef), result);
        ClipSet[clipSlot.ClipReference] = clipSlot;
        var (successfulClipRefs, errors) = ClipSet.ProcessClips(new [] {clipSlot});
        return CommandHandlerResult.CompletedResult with { SuccessfulClips = ClipSet.GetClipsFromClipReferences(successfulClipRefs).ToList(), Errors = errors };
    }
}
using System.Collections.Generic;
using System.Linq;
using Mutate4l;
using Mutateful.Compiler;
using Mutateful.Core;

namespace Mutateful.State
{
    public class ClipSet
    {
        private readonly Dictionary<ClipReference, ClipSlot> ClipSlots = new();
        public Dictionary<ClipReference, ClipSlot>.KeyCollection ClipSlotKeys => ClipSlots.Keys;

        private ClipReference InternalClipRef = new(1, 1);
        public ClipSlot this[ClipReference clipRef]
        {
            get => ClipSlots[clipRef] ?? ClipSlot.Empty;
            set => ClipSlots[clipRef] = value;
        }

        public ClipSlot this[int track, int clip]
        {
            get
            {
                InternalClipRef.Track = track;
                InternalClipRef.Clip = clip;
                return this[InternalClipRef];
            }
            set
            {
                InternalClipRef.Track = track;
                InternalClipRef.Clip = clip;
                ClipSlots[InternalClipRef] = value;
            }
        }

        // Placeholder for future use
        /*public void ApplyGuiCommand(GuiCommand command)
        {
            switch (command.Type)
            {
                case GuiCommandType.Delete:
                    // add this command to the undo stack with type and state prior to deletion
                    break;
                case GuiCommandType.SetName:
                    break;
            }
            // something like a List of InternalCommands and previous state ClipSlots could be used to support undo/redo
        }

        public void ApplyInternalCommand(InternalCommand command)
        {
            switch (command.Type)
            {
                case InternalCommandType.SetFormula:
                    ClipSlots[command.ClipSlot.ClipReference] = command.ClipSlot;
                    break;
                case InternalCommandType.SetClipData:
                    ClipSlots[command.ClipSlot.ClipReference] = command.ClipSlot;
                    break;
            }
        }*/

        private void PopulateClipReferences(ClipSlot clip)
        {
            foreach (var referencedClip in clip.Formula.AllReferencedClips)
            {
                if (clip.Formula.ClipSlotsByClipReference.ContainsKey(referencedClip))
                    clip.Formula.ClipSlotsByClipReference[referencedClip] = this[referencedClip];
                else
                    clip.Formula.ClipSlotsByClipReference.Add(referencedClip, this[referencedClip]);
            }

            var formula = clip.Formula;
            var flattenedTokenList = formula.Commands
                .SelectMany(c => c.Options.Values.SelectMany(o => o))
                .Concat(formula.Commands.SelectMany(x => x.DefaultOptionValues));
                        
            // todo: error handling
                        
            foreach (var token in flattenedTokenList.Where(t => t.IsClipReference || t.Type == TokenType.InlineClip))
            {
                token.Clip = clip.Formula.ClipSlotsByClipReference[ClipReference.FromString(token.Value)].Clip;
                token.Type = TokenType.InlineClip;
            }
        }
        
        public (List<ClipReference> successfulClips, List<string> errorMessages) ProcessClips(IEnumerable<ClipSlot> clipsToProcess)
        {
            var successfulClips = new List<ClipReference>();
            var errorMessages = new List<string>();

            foreach (var clip in clipsToProcess)
            {
                PopulateClipReferences(clip);
                        
                var processedCommand = ClipProcessor.ProcessChainedCommand(new ChainedCommand(
                    clip.Formula.Commands, 
                    clip.Formula.SourceClipReferences.Select(x => clip.Formula.ClipSlotsByClipReference[x].Clip).ToArray(), 
                    new ClipMetaData(0, (byte) clip.ClipReference.Track))
                );
                if (processedCommand.Success)
                {
                    successfulClips.Add(clip.ClipReference);
                    var processedClip = processedCommand.Result[0];
                    processedClip.ClipReference = clip.ClipReference;
                    this[clip.ClipReference].Clip = processedClip;
                }
                else
                {
                    errorMessages.Add($"Error while processing clip at {clip.ClipReference}: {processedCommand.ErrorMessage}");
                } 
            }
            return (successfulClips, errorMessages);
        }

        private IEnumerable<ClipReference> GetAllReferencedClips()
        {
            var result = new List<ClipReference>();
            foreach (var clipSlot in ClipSlots.Values.Where(clipSlot => clipSlot.Formula != Formula.Empty))
            {
                result.AddRange(clipSlot.Formula.AllReferencedClips);
            }
            return result;
        }

        public bool AllReferencedClipsValid()
        {
            var result = true;
            var clipRefs = GetAllReferencedClips();
            foreach (var clipRef in clipRefs)
            {
                if (!ClipSlots.ContainsKey(clipRef))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public bool HasCircularDependencies()
        {
            foreach (var clipRef in ClipSlots.Keys)
            {
                var clipSlot = ClipSlots[clipRef];
                var referenced = new List<ClipReference>();
                WalkClipReferences(clipSlot, referenced);
                if (referenced.Contains(clipRef))
                {
                    return true;
                }
            }
            return false;
        }

        private void WalkClipReferences(ClipSlot clipSlot, List<ClipReference> refs)
        {
            foreach (var referencedClip in clipSlot.Formula.AllReferencedClips)
            {
                refs.Add(referencedClip);
                WalkClipReferences(ClipSlots[referencedClip], refs);
            }
        }

        public Dictionary<ClipReference, List<ClipReference>> GetAllReferencedClipsByClipRef()
        {
            var result = new Dictionary<ClipReference, List<ClipReference>>();
            foreach (var clipRef in ClipSlots.Keys)
            {
                result[clipRef] = ClipSlots[clipRef].Formula.AllReferencedClips;
            }
            return result;
        }

        public IEnumerable<ClipSlot> GetClipSlotsFromClipReferences(IEnumerable<ClipReference> clipReferences)
        {
            return clipReferences.Select(x => this[x]);
        }

        public IEnumerable<Clip> GetClipsFromClipReferences(IEnumerable<ClipReference> clipReferences)
        {
            return clipReferences.Select(x => this[x].Clip);
        }
        
        private List<ClipReference> GetDependentClipsForClipRef(Dictionary<ClipReference, List<ClipReference>> referencedClipsByClipRef, ClipReference clipRef)
        {
            return referencedClipsByClipRef.Keys
                .Where(x => referencedClipsByClipRef[x].Contains(clipRef))
                .Select(x => x).ToList();
        }

        public List<ClipReference> GetAllDependentClipRefsFromClipRef(ClipReference initialClipReference)
        {
            var referencedClipsByClipRef = GetAllReferencedClipsByClipRef();
            var allDependentClips = new List<ClipReference>();
            var clipsToCheck = new List<ClipReference> {initialClipReference};
            var dependentClips = new List<ClipReference>();
            // Start with initial clip
            // 1. Find all clips (if any) that depend on this clip
            // 2. If any are found, add them to a list, then repeat from step 1
            // 3. Once no more clips are produced from this process, we have identified all dependencies from our starting clip and we return them.
            // Note that this list might not be correctly sorted yet, therefore we call GetClipReferencesInProcessableOrder afterwards to sort them properly.
            do {
                if (dependentClips.Count > 0)
                {
                    clipsToCheck.Clear();
                    clipsToCheck.AddRange(dependentClips);
                    dependentClips.Clear();
                }
                foreach (var clipReference in clipsToCheck)
                {
                    var depClips = GetDependentClipsForClipRef(referencedClipsByClipRef, clipReference);
                    dependentClips.AddRange(depClips);
                }
                allDependentClips.AddRange(dependentClips);
            } while (dependentClips.Count > 0);
            return allDependentClips;
        }
        
        public ProcessResult<List<ClipReference>> GetClipReferencesInProcessableOrder(Dictionary<ClipReference, List<ClipReference>> dependentClipsByClipRef = null)
        {
            dependentClipsByClipRef ??= GetAllReferencedClipsByClipRef();
            var sortedList = new List<ClipReference>();
            var collectionChanged = true;
            while (sortedList.Count < dependentClipsByClipRef.Keys.Count && collectionChanged)
            {
                collectionChanged = false;
                foreach (var clipRef in dependentClipsByClipRef.Keys)
                {
                    var dependentClips = dependentClipsByClipRef[clipRef];
                    if (dependentClips.Count == 0 || sortedList.Count > 0 && dependentClips.All(x => sortedList.Contains(x)))
                    {
                        if (!sortedList.Contains(clipRef)) sortedList.Add(clipRef);
                        collectionChanged = true;
                    }
                }
            }

            if (sortedList.Count < dependentClipsByClipRef.Keys.Count)
            {
                return new ProcessResult<List<ClipReference>>(
                    sortedList.Where(x => ClipSlots[x].Formula != Formula.Empty).ToList(),
                    $"Unable to process cells {string.Join(", ", dependentClipsByClipRef.Keys.Where(x => !sortedList.Contains(x)).Select(x => x.ToString()))} due to circular dependencies."
                    );
            }
            return new ProcessResult<List<ClipReference>>(sortedList.Where(x => ClipSlots[x].Formula != Formula.Empty).ToList());
          }
    }
}
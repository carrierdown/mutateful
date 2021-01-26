using System.Collections.Generic;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.State
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
        
        public List<ClipReference> GetAllReferencedClips()
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

        private Dictionary<ClipReference, List<ClipReference>> GetDependentClipsByClipRef()
        {
            var result = new Dictionary<ClipReference, List<ClipReference>>();
            foreach (var clipRef in ClipSlots.Keys)
            {
                result[clipRef] = ClipSlots[clipRef].Formula.AllReferencedClips;
            }
            return result;
        }

        public ProcessResult<List<ClipReference>> GetClipReferencesInProcessableOrder()
        {
            var dependentClipsByClipRef = GetDependentClipsByClipRef();
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
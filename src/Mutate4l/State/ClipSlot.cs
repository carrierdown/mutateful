using System;
using Mutate4l.Core;

namespace Mutate4l.State
{
    public class ClipSlot
    {
        public Clip Clip { get; set; }
        public string Name { get; }
        public Formula Formula { get; }
        
        public ClipReference ClipReference => Clip.ClipReference;
        
        public static readonly ClipSlot Empty = new ClipSlot("", Clip.Empty, Formula.Empty);
        
        public ClipSlot(string name, Clip clip, Formula formula)
        {
            Name = name;
            Clip = clip;
            Formula = formula;
        }
    }
}
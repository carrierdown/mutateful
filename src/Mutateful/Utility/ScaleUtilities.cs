using Mutateful.Core;
using static Mutateful.Utility.Scales;

namespace Mutateful.Utility
{
    public enum Scales
    {
        None = -1,
        Ionian,
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        Aeolian,
        Locrian,
        Major,
        Minor
    }

    public static class ScaleUtilities
    {
        public static readonly int[] IonianNoteIndexes = new[] {0, 2, 4, 5, 7, 9, 11};

        public static Clip GetGuideClipFromScale(Scales scale, int root, decimal length)
        {
            Clip clip = new Clip(length, true);
            root &= 0x7F;

            foreach (var ix in GetIndexesFromScale(scale))
            {
                clip.Add(new NoteEvent(root + ix, 0, length, 127));
            }
            return clip;
        }

        public static int[] GetIndexesFromScale(Scales scale)
        {
            int[] indexes = new int[7];
            int scaleRootIx = 0; // Ionian/major
            if (scale >= Ionian && scale <= Locrian)
            {
                scaleRootIx = (int) scale;
            }
            else if (scale == Minor)
            {
                scaleRootIx = (int) Aeolian;
            }

            for (var i = 0; i < IonianNoteIndexes.Length; i++)
            {
                indexes[i] = IonianNoteIndexes[(scaleRootIx + i) % IonianNoteIndexes.Length];
            }
            return indexes;
        }
    }
}
using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Mutate4l.Commands
{
    public enum Shape
    {
        Linear,
        EaseInOut,
        EaseIn
    }

    public enum RatchetMode
    {
        // whether ratcheting should be applied based on velocity or pitch in the control clip
        Velocity,
        Pitch
    }

    public class RatchetOptions
    {
        public RatchetMode Mode { get; set; } = RatchetMode.Velocity;

        [OptionInfo(min: 1, max: 20)]
        public int Min { get; set; } = 1;

        [OptionInfo(min: 1, max: 20)]
        public int Max { get; set; } = 8;

        [OptionInfo(min: 0, max: 100)]
        public int Strength { get; set; } = 100;

        public bool VelocityToStrength { get; set; }

        public Shape Shape { get; set; } = Shape.Linear;

        // Automatically scale control sequence so that lowest note corresponds to minimum ratchet value and highest note corresponds to maximum ratchet value
        public bool AutoScale { get; set; }

        public int ControlMin { get; set; } = -1; // lowest pitch/velocity for control sequence (unless AutoScale is on), e.g. pitch values 60 or lower equal Min ratchet-value.

        public int ControlMax { get; set; } = -1; // highest pitch/velocity for control sequence (unless AutoScale is on), e.g. pitch values 68 or higher equal Max ratchet-value.

        public Clip By { get; set; }
    }

    public class Ratchet
    {
        // Basic use:
        // a1 ratchet -by a2 => b2

        // Needs control sequence and target sequence(s)
        // If only one clip is specified, clip will be both control and target sequence
        public static ProcessResultArray<Clip> Apply(RatchetOptions options, params Clip[] clips)
        {
            ClipUtilities.NormalizeClipLengths((options.By != null ? clips.Prepend(options.By).ToArray() : clips));
            if (clips.Length < 2) return new ProcessResultArray<Clip>(clips);
            Clip controlSequence = new Clip(clips[0]);
            Clip[] targetSequences = clips.Skip(1).Select(x => new Clip(x)).ToArray();
            Clip[] resultSequences = new Clip[targetSequences.Count()];

            // set defaults based on mode if unset
            if (options.ControlMin == -1) options.ControlMin = options.Mode == RatchetMode.Pitch ? 60 : 20;
            if (options.ControlMax == -1) options.ControlMax = options.Mode == RatchetMode.Pitch ? 68 : 120;

            int lowestValue = options.ControlMin;
            int highestValue = options.ControlMax;
            if (lowestValue > highestValue) Utilities.Swap<int>(ref lowestValue, ref highestValue);

            if (options.AutoScale)
            {
                lowestValue = options.Mode == RatchetMode.Pitch ? controlSequence.Notes.Select(x => x.Pitch).Min() : controlSequence.Notes.Select(x => x.Velocity).Min();
                highestValue = options.Mode == RatchetMode.Pitch ? controlSequence.Notes.Select(x => x.Pitch).Max() : controlSequence.Notes.Select(x => x.Velocity).Max();
            }

            decimal controlRange = Math.Max(highestValue - lowestValue, 1);
            decimal targetRange = Math.Max(Math.Abs(options.Max - options.Min), 1);

            // set pitch for each note in control sequence
            if (options.Mode == RatchetMode.Pitch)
            {
                foreach (var note in controlSequence.Notes)
                    note.Pitch = (int)Math.Round((Math.Clamp(note.Pitch, lowestValue, highestValue) - lowestValue) / controlRange * targetRange) + options.Min;
            }
            else
            {
                foreach (var note in controlSequence.Notes)
                    note.Velocity = (int)Math.Round((Math.Clamp(note.Velocity, lowestValue, highestValue) - lowestValue) / controlRange * targetRange) + options.Min;
            }

            int i = 0;
            foreach (var targetSequence in targetSequences)
            {
                resultSequences[i++] = DoRatchet(controlSequence, targetSequence, options.Strength / 100f, options.VelocityToStrength, options.Shape, options.Mode);
            }

            return new ProcessResultArray<Clip>(resultSequences);
        }

        private static Clip DoRatchet(Clip controlSequence, Clip targetSequence, float scaleFactor, bool scaleWithVelocity, Shape shape, RatchetMode mode)
        {
            Clip result = new Clip(targetSequence.Length, targetSequence.IsLooping);

            Vector2 p1 = new Vector2(0, 0);
            // just a quick fix for now...
            Vector2 p2 = shape == Shape.EaseIn ? new Vector2(.5f, 0) : new Vector2(0, 0); //new Vector2(.47f, .09f); // controls start of curve
            Vector2 p3 = shape == Shape.EaseIn ? new Vector2(1, 1) : new Vector2(1, 1); // controls end of curve
            Vector2 p4 = new Vector2(1, 1);

            // scaling
            p2.X = p2.X * scaleFactor;
            p2.Y = p2.Y * scaleFactor;
            p3.X = 1 - ((p4.X - p3.X) * scaleFactor);
            p3.Y = 1 - ((p4.Y - p3.Y) * scaleFactor);

            Vector2[] curvePoints = new Vector2[11];
            for (int i = 0; i < curvePoints.Length; i++)
            {
                Vector2 point = CalculateBezierPoint(i / 10f, p1, p2, p3, p4);
                curvePoints[i].X = point.X;
                curvePoints[i].Y = point.Y;
            }

            foreach (var controlNote in controlSequence.Notes)
            {
                List<NoteEvent> targetNotes;
                // treating these modes the same for now - but can optionally match pitch as well when RatchetMode.Velocity is set
//                if (mode == RatchetMode.Pitch)
//                {
                    targetNotes = targetSequence.Notes.Where(x => x.StartsInsideInterval(controlNote.Start, controlNote.End) && x.Start != -1).ToList();
//                } else // velocity - pitch need to be taken into account as well
//                {
//                    targetNotes = targetSequence.Notes.Where(x => x.StartsInsideInterval(controlNote.Start, controlNote.End) && x.Pitch == controlNote.Pitch).ToList();
//                }
                targetNotes.ForEach(note =>
                    {
                        int ratchetCount = mode == RatchetMode.Pitch ? controlNote.Pitch : controlNote.Velocity;
                        decimal delta = 1.0m / ratchetCount;
                        decimal currentPos = 0;
                        for (int i = 1; i <= ratchetCount; i++)
                        {
                            decimal linearFactor = delta * i;
                            decimal curvedFactor = GetScaledValue(linearFactor, curvePoints);
                            decimal combinedFactor;
                            if (scaleWithVelocity)
                                combinedFactor = linearFactor + ((curvedFactor - linearFactor) * controlNote.Velocity / 127m);
                            else
                                combinedFactor = curvedFactor;
                            decimal currentLength = (note.Duration * combinedFactor) - currentPos;
                            result.Notes.Add(new NoteEvent(note.Pitch, note.Start + currentPos, currentLength, note.Velocity));
                            currentPos += currentLength;
                        }
                        note.Start = -1; // ensure that note is only processed once
                    }
                );
            }

            return result;
        }

        private static decimal GetScaledValue(decimal value, Vector2[] curvePoints)
        {
            if (value <= 0) return 0;
            if (value >= 1) return 1;
            int[] boundaryIndexes = FindBoundaryIndexesByX(curvePoints, (float)value);
            int lowerIndex = boundaryIndexes[0], upperIndex = boundaryIndexes[1];
            float Yval = (((float)value - curvePoints[lowerIndex].X) / (curvePoints[upperIndex].X - curvePoints[lowerIndex].X)) * (curvePoints[upperIndex].Y - curvePoints[lowerIndex].Y);
            Yval += curvePoints[lowerIndex].Y;
//            Console.WriteLine($"Point at t={value} between: {boundaryIndexes[0]}, {boundaryIndexes[1]} ({curvePoints[boundaryIndexes[0]].X} - {curvePoints[boundaryIndexes[1]].X})");
//            Console.WriteLine($"Y value for t={value}: {Yval}");
            return (decimal)Yval;
        }

        private static int[] FindBoundaryIndexesByX(Vector2[] points, float val)
        {
            int[] result = new int[2];
            int lowIndex = 0;
            int highIndex = points.Length - 1;
            for (int i = 0; i < points.Length; i++)
            {
                if (val > points[i].X) lowIndex = i;
                if (val < points[points.Length - 1 - i].X) highIndex = points.Length - 1 - i;
            }
            result[0] = lowIndex;
            result[1] = highIndex;
            return result;
        }

        private static Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * p0; //first term
            p += 3 * uu * t * p1; //second term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term

            return p;
        }
    }
}

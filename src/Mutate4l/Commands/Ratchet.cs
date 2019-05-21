using Mutate4l.Core;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Mutate4l.Cli;

namespace Mutate4l.Commands
{
    public enum Shape
    {
        Linear,
        EaseInOut,
        EaseIn,
        EaseOut
    }

    public enum RatchetMode
    {
        // whether ratcheting should be applied based on velocity or pitch in the control clip
        Velocity,
        Pitch
    }

    public class RatchetOptions
    {
        // Automatically scale control sequence so that lowest note corresponds to minimum ratchet value and highest note corresponds to maximum ratchet value
        public bool AutoScale { get; set; } // Default behaviour is that range begins at lowest note rounded to nearest octave boundary (i.e. C) and ends with highest note rounded to nearest octave boundary

        public Clip By { get; set; }

        public RatchetMode Mode { get; set; } = RatchetMode.Pitch;

        public Shape Shape { get; set; } = Shape.Linear;

        public decimal Strength { get; set; } = 1;

        public bool VelocityToStrength { get; set; }

        [OptionInfo(type: OptionType.Default)]
        public int[] RatchetValues { get; set; } = new int[0];
    }

    public static class Ratchet
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out RatchetOptions options);
            return !success ? new ProcessResultArray<Clip>(msg) : Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(RatchetOptions options, params Clip[] clips)
        {
            options.Strength = Math.Clamp(0, 1, options.Strength);
            if (options.By != null)
            {
                clips = clips.Prepend(options.By).ToArray();
            }
            ClipUtilities.NormalizeClipLengths(clips);
            if (clips.Length < 2)
            {
                clips = new[] { clips[0], clips[0] };
            }
            Clip controlSequence = new Clip(clips[0]);
            Clip[] targetSequences = clips.Skip(1).Select(x => new Clip(x)).ToArray();
            Clip[] resultSequences = new Clip[targetSequences.Length];

            if (options.RatchetValues.Length > 0)
            {
                for (var i = 0; i < targetSequences.Length; i++)
                {
                    var targetSequence = targetSequences[i];
                    resultSequences[i] = DoManualRatchet(options.RatchetValues, targetSequence, options.Strength, options.VelocityToStrength, options.Shape,
                        options.Mode);
                }
            }
            else
            {
                var (controlMin, controlMax, targetRange) = options.Mode == RatchetMode.Pitch
                    ? GetControlValuesFromPitch(options, controlSequence)
                    : GetControlValuesFromVelocity(options, controlSequence);
                
                float controlRange = Math.Max(controlMax - controlMin, 1);

                // set pitch for each note in control sequence
                if (options.Mode == RatchetMode.Pitch)
                {
                    foreach (var note in controlSequence.Notes)
                        note.Pitch = (int)Math.Round((note.Pitch - controlMin) / controlRange * targetRange) + 1;
                }
                else
                {
                    foreach (var note in controlSequence.Notes)
                        note.Velocity = (int)Math.Round((Math.Clamp(note.Velocity, controlMin, controlMax) - controlMin) / controlRange * targetRange) + 1;
                }
                for (var i = 0; i < targetSequences.Length; i++)
                {
                    var targetSequence = targetSequences[i];
                    resultSequences[i] = DoRatchet(controlSequence, targetSequence, (float) options.Strength, options.VelocityToStrength, options.Shape,
                        options.Mode);
                }
            }


            return new ProcessResultArray<Clip>(resultSequences);
        }

        private static (int controlMin, int controlMax, int targetRange) GetControlValuesFromVelocity(RatchetOptions options, Clip controlSequence)
        {
            int controlMin;
            int controlMax;
            int targetRange;
            if (options.AutoScale)
            {
                controlMin = controlSequence.Notes.Select(x => x.Velocity).Min();
                controlMax = controlSequence.Notes.Select(x => x.Velocity).Max();
                targetRange = Math.Max(controlMax - controlMin, 10) / 10;
            }
            else
            {
                controlMin = 20;
                controlMax = 120;
                targetRange = 16;
            }

            return (controlMin, controlMax, targetRange);
        }

        private static (int controlMin, int controlMax, int targetRange) GetControlValuesFromPitch(RatchetOptions options, Clip controlSequence)
        {
            int controlMin;
            int controlMax;
            int min = controlSequence.Notes.Select(x => x.Pitch).Min();
            int max = controlSequence.Notes.Select(x => x.Pitch).Max();
            if (options.AutoScale)
            {
                controlMin = min;
                controlMax = max;
            }
            else
            {
                controlMin = min - min % 12;
                controlMax = max + 12 - max % 12;
            }

            var targetRange = Math.Max(controlMax - controlMin, 1);
            return (controlMin, controlMax, targetRange);
        }

        private static Clip DoManualRatchet(IReadOnlyList<int> ratchetValues, Clip targetSequence, decimal scaleFactor, bool scaleWithVelocity, Shape shape, RatchetMode mode)
        {
            var result = new Clip(targetSequence.Length, targetSequence.IsLooping);
            var curvePoints = GetCurvePoints((float)scaleFactor, shape);

            for (var i = 0; i < targetSequence.Notes.Count; i++)
            {
                AddRatchets(result, targetSequence.Notes[i], ratchetValues[i % ratchetValues.Count], scaleWithVelocity, curvePoints, scaleFactor);
            }
            return result;
        }
        
        private static Clip DoRatchet(Clip controlSequence, Clip targetSequence, float scaleFactor, bool scaleWithVelocity, Shape shape, RatchetMode mode)
        {
            var result = new Clip(targetSequence.Length, targetSequence.IsLooping);
            var curvePoints = GetCurvePoints(scaleFactor, shape);

            foreach (var controlNote in controlSequence.Notes)
            {
                // treating these modes the same for now - but can optionally match pitch as well when RatchetMode.Velocity is set
//                if (mode == RatchetMode.Pitch)
//                {
                var targetNotes = targetSequence.Notes.Where(x => x.StartsInsideInterval(controlNote.Start, controlNote.End) && x.Start != -1).ToList();
//                } else // velocity - pitch need to be taken into account as well
//                {
//                    targetNotes = targetSequence.Notes.Where(x => x.StartsInsideInterval(controlNote.Start, controlNote.End) && x.Pitch == controlNote.Pitch).ToList();
//                }
                targetNotes.ForEach(note =>
                {
                    var ratchetCount = mode == RatchetMode.Pitch ? 
                        controlNote.Pitch : 
                        controlNote.Velocity;
                    AddRatchets(result, note, ratchetCount, scaleWithVelocity, curvePoints, note.Velocity / 127m);
                });
            }

            return result;
        }

        private static Vector2[] GetCurvePoints(float scaleFactor, Shape shape)
        {
            var (p1, p2, p3, p4) = GetCurveControlPoints(shape);
            p2.X *= scaleFactor;
            p2.Y *= scaleFactor;
            p3.X = 1 - (p4.X - p3.X) * scaleFactor;
            p3.Y = 1 - (p4.Y - p3.Y) * scaleFactor;

            Vector2[] curvePoints = new Vector2[11];
            for (var i = 0; i < curvePoints.Length; i++)
            {
                Vector2 point = CalculateBezierPoint(i / 10f, p1, p2, p3, p4);
                curvePoints[i].X = point.X;
                curvePoints[i].Y = point.Y;
            }
            return curvePoints;
        }

        private static (Vector2 Point1, Vector2 Point2, Vector2 Point3, Vector2 Point4) GetCurveControlPoints(Shape shape)
        {
            switch (shape)
            {
                case Shape.EaseIn:
                    return (new Vector2(0, 0), new Vector2(.55f, 0), new Vector2(1, 1), new Vector2(1, 1));
                case Shape.EaseOut:
                    return (new Vector2(0, 0), new Vector2(0, 0), new Vector2(.5f, 1), new Vector2(1, 1));
                case Shape.EaseInOut:
                    return (new Vector2(0, 0), new Vector2(.55f, 0), new Vector2(.45f, 1), new Vector2(1, 1));
            }
            // linear
            return (new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1));
        }

        private static void AddRatchets(Clip result, NoteEvent note, int ratchetCount, bool scaleWithVelocity, Vector2[] curvePoints, decimal scaleFactor = 1)
        {
            decimal delta = 1.0m / ratchetCount;
            decimal currentPos = 0;
            for (var i = 1; i <= ratchetCount; i++)
            {
                decimal linearFactor = delta * i;
                decimal curvedFactor = GetScaledValue(linearFactor, curvePoints);
                decimal combinedFactor;
                if (scaleWithVelocity)
                    combinedFactor = linearFactor + (curvedFactor - linearFactor) * scaleFactor;
                else
                    combinedFactor = curvedFactor;
                decimal currentLength = note.Duration * combinedFactor - currentPos;
                result.Notes.Add(new NoteEvent(note.Pitch, note.Start + currentPos, currentLength, note.Velocity));
                currentPos += currentLength;
            }
            note.Start = -1; // ensure that note is only processed once
        }

        private static decimal GetScaledValue(decimal value, Vector2[] curvePoints)
        {
            if (value <= 0) return 0;
            if (value >= 1) return 1;
            int[] boundaryIndexes = FindBoundaryIndexesByX(curvePoints, (float)value);
            int lowerIndex = boundaryIndexes[0], 
                upperIndex = boundaryIndexes[1];
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

            Vector2 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}

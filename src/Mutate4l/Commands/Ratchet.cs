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

        [OptionInfo(min: 0, max: 100)]
        public int Strength { get; set; } = 100;

        public bool VelocityToStrength { get; set; }

        [OptionInfo(type: OptionType.Default)]
        public int[] RatchetValues { get; set; } = new int[0];
    }

    public static class Ratchet
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out RatchetOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        // Basic use:
        // a1 ratchet -by a2 => b2

        // Needs control sequence and target sequence(s)
        // If only one clip is specified, clip will be both control and target sequence
        public static ProcessResultArray<Clip> Apply(RatchetOptions options, params Clip[] clips)
        {
            ClipUtilities.NormalizeClipLengths((options.By != null ? clips.Prepend(options.By).ToArray() : clips));
            if (clips.Length < 2)
            {
                clips = new Clip[] { clips[0], clips[0] };
            }
            Clip controlSequence = new Clip(clips[0]);
            Clip[] targetSequences = clips.Skip(1).Select(x => new Clip(x)).ToArray();
            Clip[] resultSequences = new Clip[targetSequences.Count()];

            if (options.RatchetValues.Length > 0)
            {
                options.Mode = RatchetMode.Pitch; // force pitch-mode if manual values are entered
                controlSequence.Notes = new SortedList<NoteEvent>();
                var y = 0;
                var targetSeq = targetSequences[0];
                var clampedValues = options.RatchetValues.Select(x => Math.Clamp(x, 1, 20)).ToArray(); // absolute max is 20 when ratchets are manually specified. Control range is set to 10 unless higher ratchet values than 10 are specified.
                foreach (var value in clampedValues)
                {
                    controlSequence.Add(
                        new NoteEvent(options.Mode == RatchetMode.Pitch ? value : targetSeq.Notes[y % targetSeq.Count].Pitch, 
                            targetSeq.Notes[y % targetSeq.Count].Start, 
                            targetSeq.Notes[y % targetSeq.Count].Duration,
                            options.Mode == RatchetMode.Velocity ? value : targetSeq.Notes[y % targetSeq.Count].Velocity)
                    );
                    y++;
                }
            }
            else
            {
                int controlMin, 
                    controlMax;
                int targetRange;

                if (options.Mode == RatchetMode.Pitch)
                {
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
                    if (options.RatchetValues.Length > 0) // override controlrange logic if ratchets are manually specified
                    {
                        controlMin = 1;
                        controlMax = Math.Max(10, options.RatchetValues.Max());
                    }
                    targetRange = Math.Max(controlMax - controlMin, 1);
                }
                else
                {
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
                }
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

            var (p1, p2, p3, p4) = GetCurveControlPoints(shape);
            p2.X = p2.X * scaleFactor;
            p2.Y = p2.Y * scaleFactor;
            p3.X = 1 - (p4.X - p3.X) * scaleFactor;
            p3.Y = 1 - (p4.Y - p3.Y) * scaleFactor;

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

            Vector2 p = uuu * p0; //first term
            p += 3 * uu * t * p1; //second term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term

            return p;
        }
    }
}

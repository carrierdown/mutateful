using Mutateful.Commands;
using Mutateful.Core;
using Mutateful.Utility;
using NUnit.Framework;

namespace MutatefulTests;

[TestFixture]
public class CommandTests
{
    [Test]
    public void TestConstrainNoteEventPitch()
    {
        var clip1 = new Clip(4, true)
        {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(60, 0, .5m, 100), // C
                new NoteEvent(55, 1, .5m, 100), // G
                new NoteEvent(62, 2, .5m, 100)  // D
            }
        };
        var clip2 = new Clip(4, true)
        {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(47, 0, .5m, 100), // B
                new NoteEvent(63, 3, .5m, 100), // D#
                new NoteEvent(81, 4, .5m, 100)  // A
            }
        };
        var resultObj = Scale.Apply(new ScaleOptions(), clip1, clip2);
        Assert.IsTrue(resultObj.Success);
        var result = resultObj.Result[0];
        Assert.AreEqual(48, result.Notes[0].Pitch);
        Assert.AreEqual(62, result.Notes[1].Pitch);
        Assert.AreEqual(79, result.Notes[2].Pitch);
    }
    
    [Test]
        public void TestInterleaveTimeRange()
        {
            var clip1 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, 4, 100)
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(62, 0, 4, 100)
                }
            };
            var options = new InterleaveOptions
            {
                Ranges = new decimal[] { 1, 1 },
                Mode = InterleaveMode.Time
            };
            var resultObj = Interleave.Apply(options, new ClipMetaData(), clip1, clip2);
            Assert.IsTrue(resultObj.Success);
            Assert.IsTrue(resultObj.Result.Length == 1);
            var result = resultObj.Result[0];

            for (var i = 0; i < 8; i++)
            {
                Console.WriteLine($"{result.Notes[i].Start} {result.Notes[i].Pitch}");
                Assert.AreEqual(result.Notes[i].Pitch, i % 2 == 0 ? 60 : 62);
                Assert.AreEqual(result.Notes[i].Start, i);
                Assert.AreEqual(result.Notes[0].Duration, 1);
            }
        }

        [Test]
        public void TestInterleaveTimeRangesAndCounts()
        {
            var clip1 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, 4, 100)
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(62, 0, 4, 100)
                }
            };
            var options = new InterleaveOptions
            {
                Ranges = new decimal[] { 2, 1 },
                Repeats = new int[] { 1, 2 },
                Mode = InterleaveMode.Time
            };

            var resultObj = Interleave.Apply(options, new ClipMetaData(), clip1, clip2);
            Assert.IsTrue(resultObj.Success);
            Assert.IsTrue(resultObj.Result.Length == 1);
            var result = resultObj.Result[0];
            Assert.AreEqual(16, result.Length);
            Assert.AreEqual(12, result.Notes.Count);
            Assert.AreEqual(60, result.Notes[0].Pitch);
            Assert.AreEqual(2, result.Notes[0].Duration);
            Assert.AreEqual(62, result.Notes[1].Pitch);
            Assert.AreEqual(1, result.Notes[1].Duration);
            Assert.AreEqual(62, result.Notes[2].Pitch);
            Assert.AreEqual(1, result.Notes[2].Duration);
            Assert.AreEqual(60, result.Notes[3].Pitch);
            Assert.AreEqual(2, result.Notes[3].Duration);
            Assert.AreEqual(62, result.Notes[4].Pitch);
            Assert.AreEqual(1, result.Notes[4].Duration);
            Assert.AreEqual(62, result.Notes[5].Pitch);
            Assert.AreEqual(1, result.Notes[5].Duration);
        }

        [Test]
        public void TestInterleaveTimeRange2()
        {
            var clip1 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, .4m, .8m, 100),
                   new NoteEvent(60, 1.3m, 2, 100),
                   new NoteEvent(60, 3.3m, .7m, 100)
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(62, 0, 4, 100)
                }
            };
            var options = new InterleaveOptions
            {
                Ranges = new decimal[] { 1, 1 },
                Mode = InterleaveMode.Time
            };
            var resultObj = Interleave.Apply(options, new ClipMetaData(), clip1, clip2);
            var result = resultObj.Result[0];
            Assert.AreEqual(8, result.Length);
            Assert.AreEqual(60, result.Notes[0].Pitch);
            Assert.AreEqual(.4m, result.Notes[0].Start);
            Assert.AreEqual(.6m, result.Notes[0].Duration);
            Assert.AreEqual(62, result.Notes[1].Pitch);
            Assert.AreEqual(1, result.Notes[1].Start);
            Assert.AreEqual(1, result.Notes[1].Duration);
            Assert.AreEqual(60, result.Notes[2].Pitch);
            Assert.AreEqual(2, result.Notes[2].Start);
            Assert.AreEqual(0.2m, result.Notes[2].Duration);
            Assert.AreEqual(60, result.Notes[3].Pitch);
            Assert.AreEqual(2.3m, result.Notes[3].Start);
            Assert.AreEqual(0.7m, result.Notes[3].Duration);
            Assert.AreEqual(62, result.Notes[4].Pitch);
            Assert.AreEqual(3, result.Notes[4].Start);
            Assert.AreEqual(1, result.Notes[4].Duration);
            Assert.AreEqual(60, result.Notes[5].Pitch);
            Assert.AreEqual(4, result.Notes[5].Start);
            Assert.AreEqual(1, result.Notes[5].Duration);
            Assert.AreEqual(62, result.Notes[6].Pitch);
            Assert.AreEqual(5, result.Notes[6].Start);
            Assert.AreEqual(1, result.Notes[6].Duration);
            Assert.AreEqual(60, result.Notes[7].Pitch);
            Assert.AreEqual(6, result.Notes[7].Start);
            Assert.AreEqual(.3m, result.Notes[7].Duration);
            Assert.AreEqual(60, result.Notes[8].Pitch);
            Assert.AreEqual(6.3m, result.Notes[8].Start);
            Assert.AreEqual(.7m, result.Notes[8].Duration);
            Assert.AreEqual(62, result.Notes[9].Pitch);
            Assert.AreEqual(7, result.Notes[9].Start);
            Assert.AreEqual(1, result.Notes[9].Duration);
        }

        [Test]
        public void TestInterleaveEventCount()
        {
            var clip1 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, 1, 100),
                   new NoteEvent(60, 1, 1, 100),
                   new NoteEvent(60, 2, 1, 100),
                   new NoteEvent(60, 3, 1, 100)
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(62, 0, 1, 100),
                   new NoteEvent(62, 1, 1, 100),
                   new NoteEvent(62, 2, 1, 100),
                   new NoteEvent(62, 3, 1, 100)
                }
            };
            var options = new InterleaveOptions
            {
                Mode = InterleaveMode.Event
            };
            var resultObj = Interleave.Apply(options, new ClipMetaData(), clip1, clip2);
            Assert.IsTrue(resultObj.Success);
            Assert.IsTrue(resultObj.Result.Length == 1);
            var clip = resultObj.Result[0];
            Assert.AreEqual(8m, clip.Length);
            for (var i = 0; i < 8; i++)
            {
                Console.WriteLine($"{clip.Notes[i].Start} {clip.Notes[i].Pitch}");
                Assert.AreEqual(i % 2 == 0 ? 60 : 62, clip.Notes[i].Pitch);
                Assert.AreEqual(i, clip.Notes[i].Start);
                Assert.AreEqual(1, clip.Notes[0].Duration);
            }
        }
        
        [Test]
        public void TestRatchetPitchWithByClip()
        {
            byte[] input = {97, 0, 0, 2, 0, 0, 0, 0, 128, 64, 1, 6, 0, 65, 0, 0, 160, 63, 0, 0, 0, 63, 100, 65, 0, 0, 0, 64, 0, 0, 128, 63, 100, 67, 0, 0, 64, 64, 0, 0, 0, 63, 100, 68, 0, 0, 0, 0, 0, 0, 64, 63, 100, 69, 0, 0, 128, 63, 0, 0, 0, 63, 100, 72, 0, 0, 96, 64, 0, 0, 0, 63, 100, 0, 1, 0, 0, 128, 64, 1, 5, 0, 60, 0, 0, 0, 0, 0, 0, 64, 63, 100, 62, 0, 0, 128, 63, 0, 0, 64, 63, 100, 62, 0, 0, 64, 64, 0, 0, 0, 63, 100, 64, 0, 0, 0, 64, 0, 0, 128, 63, 100, 72, 0, 0, 96, 64, 0, 0, 0, 63, 100, 91, 48, 93, 32, 114, 97, 116, 99, 104, 101, 116, 32, 45, 98, 121, 32, 91, 49, 93};
            byte[] output = {97, 0, 0, 0, 128, 64, 1, 28, 0, 68, 0, 0, 0, 0, 0, 0, 64, 63, 100, 69, 0, 0, 128, 63, 170, 170, 42, 62, 100, 69, 85, 85, 149, 63, 173, 170, 42, 62, 100, 65, 0, 0, 160, 63, 170, 170, 42, 62, 100, 69, 171, 170, 170, 63, 170, 170, 42, 62, 100, 65, 85, 85, 181, 63, 173, 170, 42, 62, 100, 65, 171, 170, 202, 63, 170, 170, 42, 62, 100, 65, 0, 0, 0, 64, 205, 204, 76, 62, 100, 65, 205, 204, 12, 64, 205, 204, 76, 62, 100, 65, 154, 153, 25, 64, 205, 204, 76, 62, 100, 65, 102, 102, 38, 64, 205, 204, 76, 62, 100, 65, 51, 51, 51, 64, 205, 204, 76, 62, 100, 67, 0, 0, 64, 64, 170, 170, 42, 62, 100, 67, 171, 170, 74, 64, 173, 170, 42, 62, 100, 67, 85, 85, 85, 64, 170, 170, 42, 62, 100, 72, 0, 0, 96, 64, 217, 137, 29, 61, 100, 72, 39, 118, 98, 64, 222, 137, 29, 61, 100, 72, 79, 236, 100, 64, 206, 137, 29, 61, 100, 72, 118, 98, 103, 64, 220, 137, 29, 61, 100, 72, 158, 216, 105, 64, 220, 137, 29, 61, 100, 72, 197, 78, 108, 64, 220, 137, 29, 61, 100, 72, 236, 196, 110, 64, 220, 137, 29, 61, 100, 72, 20, 59, 113, 64, 206, 137, 29, 61 , 100, 72, 59, 177, 115, 64, 220, 137, 29, 61, 100, 72, 98, 39, 118, 64, 220, 137, 29, 61, 100, 72, 138, 157, 120, 64, 220, 137, 29, 61, 100, 72, 177, 19, 123, 64, 206, 137, 29, 61, 100, 72, 217, 137, 125, 64, 220, 137, 29, 61, 100};
            
            TestUtilities.InputShouldProduceGivenOutput(input, output);
        }
        
        // Test generated by mutate4l from formula: [0] ratchet 1 2 3 5
        [Test]
        public void TestRatchetDirectValues()
        {
            byte[] input = { 98, 0, 0, 1, 0, 0, 0, 0, 128, 64, 1, 6, 0, 65, 0, 0, 160, 63, 0, 0, 0, 63, 100, 65, 0, 0, 0, 64, 0, 0, 128, 63, 100, 67, 0, 0, 64, 64, 0, 0, 0, 63, 100, 68, 0, 0, 0, 0, 0, 0, 64, 63, 100, 69, 0, 0, 128, 63, 0, 0, 0, 63, 100, 72, 0, 0, 96, 64, 0, 0, 0, 63, 100, 91, 48, 93, 32, 114, 97, 116, 99, 104, 101, 116, 32, 49, 32, 50, 32, 51, 32, 53 };
            byte[] output = { 98, 0, 0, 0, 128, 64, 1, 14, 0, 68, 0, 0, 0, 0, 0, 0, 64, 63, 100, 69, 0, 0, 128, 63, 0, 0, 128, 62, 100, 65, 0, 0, 160, 63, 170, 170, 42, 62, 100, 69, 0, 0, 160, 63, 0, 0, 128, 62, 100, 65, 85, 85, 181, 63, 173, 170, 42, 62, 100, 65, 171, 170, 202, 63, 170, 170, 42, 62, 100, 65, 0, 0, 0, 64, 205, 204, 76, 62, 100, 65, 205, 204, 12, 64, 205, 204, 76, 62, 100, 65, 154, 153, 25, 64, 205, 204, 76, 62, 100, 65, 102, 102, 38, 64, 205, 204, 76, 62, 100, 65, 51, 51, 51, 64, 205, 204, 76, 62, 100, 67, 0, 0, 64, 64, 0, 0, 0, 63, 100, 72, 0, 0, 96, 64, 0, 0, 128, 62, 100, 72, 0, 0, 112, 64, 0, 0, 128, 62, 100 };

            TestUtilities.InputShouldProduceGivenOutput(input, output);
        }
        
        [Test]
        public void TestShuffle()
        {
            var clip = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, .5m, 100),
                   new NoteEvent(62, .5m, .5m, 100),
                   new NoteEvent(64, 1, .5m, 100),
                   new NoteEvent(66, 1.5m, .5m, 100),
                   new NoteEvent(68, 2, .5m, 100),
                   new NoteEvent(70, 2.5m, .5m, 100)
                }
            };
            var byClip = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                    new NoteEvent(40, 0, .5m, 100),
                    new NoteEvent(41, 1, .5m, 100),
                    new NoteEvent(42, 2, .5m, 100)
                }
            };
            var options = new ShuffleOptions()
            {
                By = byClip
            };
            var resultObj = Shuffle.Apply(options, clip);
            Assert.IsTrue(resultObj.Success);
            var result = resultObj.Result[0];
            Assert.AreEqual(60, result.Notes[0].Pitch);
            Assert.AreEqual(64, result.Notes[1].Pitch);
            Assert.AreEqual(68, result.Notes[2].Pitch);
            Assert.AreEqual(62, result.Notes[3].Pitch);
            Assert.AreEqual(66, result.Notes[4].Pitch);
            Assert.AreEqual(70, result.Notes[5].Pitch);
        }

        //[Test]
        public void TestShuffleComplex()
        {
            var clip = IOUtilities.StringToClip("8 True 36 0.00000 0.25000 100 39 0.00000 0.25000 100 42 0.25000 0.25000 100 41 0.50000 0.25000 100 36 0.75000 0.25000 100 39 0.75000 0.25000 100 38 1.00000 0.25000 100 39 1.25000 0.25000 100 42 1.50000 0.25000 100 38 1.75000 0.25000 100 42 2.00000 0.25000 100 38 2.50000 0.25000 100 50 2.50000 0.75000 100 42 3.00000 0.25000 100 38 3.25000 0.25000 100 50 3.25000 0.50000 100 36 4.00000 0.25000 100 39 4.00000 0.25000 100 42 4.25000 0.25000 100 41 4.50000 0.25000 100 36 4.75000 0.25000 100 39 4.75000 0.25000 100 38 5.00000 0.25000 100 42 5.25000 0.25000 100 39 5.50000 0.25000 100 38 5.75000 0.25000 100 42 6.00000 0.25000 100 48 6.25000 0.25000 100 38 6.50000 0.25000 100 37 6.75000 0.25000 100 42 7.00000 0.25000 100 48 7.25000 0.25000 100 44 7.50000 0.25000 100 38 7.75000 0.25000 100");
            var byClip = new Clip(4, true) {
                Notes = new SortedList<NoteEvent>()
                {
                    new NoteEvent(40, 0, .5m, 100),
                    new NoteEvent(49, 1, .5m, 100),
                    new NoteEvent(50, 2, .5m, 100)
                }
            };
            var resultObj = Shuffle.Apply(new ShuffleOptions() { By = byClip }, clip);

        }

        [Test]
        public void TestShuffleGrouped()
        {
            var clip = new Clip(4, true) {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, .5m, 100),
                   new NoteEvent(61, .5m, .5m, 100),
                   new NoteEvent(62, 1, .5m, 100),
                   new NoteEvent(63, 1.5m, .5m, 100),
                   new NoteEvent(64, 2, .5m, 100),
                   new NoteEvent(65, 2.5m, 1.5m, 100),
                   new NoteEvent(70, 2.5m, .5m, 100),
                   new NoteEvent(66, 3m, .5m, 100)
                }
            };
            var byClip = new Clip(4, true) {
                Notes = new SortedList<NoteEvent>()
                {
                    new NoteEvent(40, 0, .5m, 100),
                    new NoteEvent(44, 1, .5m, 100),
                    new NoteEvent(40, 2, .5m, 100)
                }
            };
            var resultObj = Shuffle.Apply(new ShuffleOptions() {
                By = byClip
            }, clip);
            Assert.IsTrue(resultObj.Success);
            var result = resultObj.Result[0];
            Assert.AreEqual(60, result.Notes[0].Pitch);
            Assert.AreEqual(65, result.Notes[1].Pitch);
            Assert.AreEqual(70, result.Notes[2].Pitch);
            Assert.AreEqual(61, result.Notes[3].Pitch);
            Assert.AreEqual(62, result.Notes[4].Pitch);
        }
}
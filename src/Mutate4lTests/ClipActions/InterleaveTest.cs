using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Commands;
using Mutate4l.Core;
using Mutate4l.Dto;
using System;
using static Mutate4l.Commands.Interleave;

namespace Mutate4lTests.ClipActions
{
    [TestClass]
    public class InterleaveTest
    {
        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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
    }
}

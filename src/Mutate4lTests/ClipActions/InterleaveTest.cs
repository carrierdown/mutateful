using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.ClipActions;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;

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
                Notes = new List<Note>()
                {
                   new Note(60, 0, 4, 100)
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new List<Note>()
                {
                   new Note(62, 0, 4, 100)
                }
            };
            Interleave.EventRangeA = 1;
            Interleave.EventRangeB = 1;
            Interleave.Mode = InterleaveMode.TimeRange;
            var result = Interleave.Apply(clip1, clip2);

            for (var i = 0; i < 8; i++)
            {
                Console.WriteLine($"{result.Notes[i].Start} {result.Notes[i].Pitch}");
                Assert.AreEqual(result.Notes[i].Pitch, i % 2 == 0 ? 60 : 62);
                Assert.AreEqual(result.Notes[i].Start, i);
                Assert.AreEqual(result.Notes[0].Duration, 1);
            }
        }

        [TestMethod]
        public void TestInterleaveEventCount()
        {
            var clip1 = new Clip(4, true)
            {
                Notes = new List<Note>()
                {
                   new Note(60, 0, 1, 100),
                   new Note(60, 1, 1, 100),
                   new Note(60, 2, 1, 100),
                   new Note(60, 3, 1, 100)
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new List<Note>()
                {
                   new Note(62, 0, 1, 100),
                   new Note(62, 1, 1, 100),
                   new Note(62, 2, 1, 100),
                   new Note(62, 3, 1, 100)
                }
            };
            Interleave.EventRangeA = 1;
            Interleave.EventRangeB = 1;
            Interleave.Mode = InterleaveMode.EventCount;
            var result = Interleave.Apply(clip1, clip2);

            for (var i = 0; i < 8; i++)
            {
                Console.WriteLine($"{result.Notes[i].Start} {result.Notes[i].Pitch}");
                Assert.AreEqual(result.Notes[i].Pitch, i % 2 == 0 ? 60 : 62);
                Assert.AreEqual(result.Notes[i].Start, i);
                Assert.AreEqual(result.Notes[0].Duration, 1);
            }
        }
    }
}

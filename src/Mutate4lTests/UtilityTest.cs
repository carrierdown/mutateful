using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Core;
using Mutate4l.Utility;
using System;

namespace Mutate4lTests
{
    [TestClass]
    public class UtilityTest
    {
        [TestMethod]
        public void TestNormalizeClipLengths()
        {
            var clip1 = new Clip(1, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, .25m, 100),
                   new NoteEvent(60, .4m, .1m, 100),
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
            var clip3 = new Clip(2, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(64, .5m, 1, 100),
                   new NoteEvent(64, 1.5m, .5m, 100),
                }
            };
            ClipUtilities.NormalizeClipLengths(clip1, clip2, clip3);
            Assert.AreEqual(clip1.Notes[0].Start, 0);
            Assert.AreEqual(clip1.Notes[1].Start, .4m);
            Assert.AreEqual(clip1.Notes[2].Start, 1);
            Assert.AreEqual(clip1.Notes[3].Start, 1.4m);
            Assert.AreEqual(clip1.Notes[4].Start, 2);
            Assert.AreEqual(clip1.Notes[5].Start, 2.4m);
            Assert.AreEqual(clip1.Notes[6].Start, 3);
            Assert.AreEqual(clip1.Notes[7].Start, 3.4m);
            Assert.AreEqual(clip1.Notes.Count, 8);
            Assert.AreEqual(clip2.Notes[0].Start, 0);
            Assert.AreEqual(clip2.Notes[1].Start, 1);
            Assert.AreEqual(clip2.Notes[2].Start, 2);
            Assert.AreEqual(clip2.Notes[3].Start, 3);
            Assert.AreEqual(clip2.Notes.Count, 4);
            Assert.AreEqual(clip3.Notes[0].Start, .5m);
            Assert.AreEqual(clip3.Notes[1].Start, 1.5m);
            Assert.AreEqual(clip3.Notes[2].Start, 2.5m);
            Assert.AreEqual(clip3.Notes[3].Start, 3.5m);
            Assert.AreEqual(clip3.Notes.Count, 4);
        }

        [TestMethod]
        public void TestMonophonize()
        {
            var clip1 = new Clip(1, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, 1, 100),
                   new NoteEvent(62, 0, 0.2m, 100),
                   new NoteEvent(62, 0.3m, 0.2m, 100),
                   new NoteEvent(62, 0.5m, 0.2m, 100),
                   new NoteEvent(62, 0.8m, 0.2m, 100),
                   new NoteEvent(60, 1, 1, 100),
                }
            };
            Assert.AreEqual(6, clip1.Notes.Count);
            ClipUtilities.Monophonize(clip1);
            Assert.AreEqual(2, clip1.Notes.Count);

            clip1 = new Clip(1, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, .2m, 100),
                   new NoteEvent(62, .1m, .2m, 100),
                   new NoteEvent(62, .2m, .2m, 100)
                }
            };
            Assert.AreEqual(3, clip1.Notes.Count);
            ClipUtilities.Monophonize(clip1);
            Assert.AreEqual(2, clip1.Notes.Count);
        }

        //[TestMethod]
        public void MyTestMethod()
        {
            for (var i = 10; i < 36; i += 5)
            {
                Console.WriteLine($"pitch {i} nearest: {ClipUtilities.FindNearestNotePitchInSet(i, new int[] { 0,12,24,32 })}");
            }
            
        }
    }
}

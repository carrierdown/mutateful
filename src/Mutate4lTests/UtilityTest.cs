using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Dto;
using Mutate4l.Core;
using Mutate4l.Utility;

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
                Notes = new SortedList<Note>()
                {
                   new Note(60, 0, .25m, 100),
                   new Note(60, .4m, .1m, 100),
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new SortedList<Note>()
                {
                   new Note(62, 0, 1, 100),
                   new Note(62, 1, 1, 100),
                   new Note(62, 2, 1, 100),
                   new Note(62, 3, 1, 100)
                }
            };
            var clip3 = new Clip(2, true)
            {
                Notes = new SortedList<Note>()
                {
                   new Note(64, .5m, 1, 100),
                   new Note(64, 1.5m, .5m, 100),
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
    }
}

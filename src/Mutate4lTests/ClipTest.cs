using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Dto;
using Mutate4l.Core;
using Mutate4l.Utility;
using System.Linq;

namespace Mutate4lTests
{
    [TestClass]
    public class ClipTest
    {
        [TestMethod]
        public void TestGroupSimultaneousNotesAndFlattenNotes()
        {
            var clip = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, 2m, 100),
                   new NoteEvent(62, .5m, .5m, 100),
                   new NoteEvent(62, 1, .5m, 100),
                   new NoteEvent(65, 1, 1m, 100),
                   new NoteEvent(60, 2, .5m, 100),
                   new NoteEvent(62, 2.5m, .5m, 100),
                   new NoteEvent(66, 2.5m, .5m, 100)
                }
            };
            clip.GroupSimultaneousNotes();

            Assert.AreEqual(clip.Notes[0].Start, 0);
            Assert.AreEqual(clip.Notes[1].Start, .5m);
            Assert.AreEqual(clip.Notes[2].Start, 1);
            Assert.AreEqual(clip.Notes[3].Start, 2);
            Assert.AreEqual(clip.Notes[4].Start, 2.5m);
            
            clip.Flatten();

            Assert.AreEqual(clip.Notes[0].Start, 0);
            Assert.AreEqual(clip.Notes[1].Start, .5m);
            Assert.AreEqual(clip.Notes[2].Start, 1);
            Assert.AreEqual(clip.Notes[3].Start, 1);
            Assert.AreEqual(clip.Notes[4].Start, 2);
            Assert.AreEqual(clip.Notes[5].Start, 2.5m);
            Assert.AreEqual(clip.Notes[6].Start, 2.5m);
        }
    }
}

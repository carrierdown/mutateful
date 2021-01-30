using Mutate4l.Core;
using NUnit.Framework;

namespace Mutate4lTests.Core
{
    [TestFixture]
    public class SortedListTest
    {
        [Test]
        public void TestSortedList()
        {
            var sortedList = new SortedList<NoteEvent>();
            sortedList.Add(new NoteEvent(64, 1, 1, 100));
            sortedList.Add(new NoteEvent(60, 0, 1, 100));
            sortedList.Add(new NoteEvent(60, .5m, 1, 100));
            sortedList.Add(new NoteEvent(60, .1m, 1, 100));
            sortedList.Add(new NoteEvent(60, 2.00001m, 1, 100));
            sortedList.Add(new NoteEvent(60, 7, 1, 100));
            sortedList.Add(new NoteEvent(60, 12.5m, 1, 100));
            sortedList.Add(new NoteEvent(60, 10, 1, 100));
            sortedList.Add(new NoteEvent(60, 2, 1, 100));
            sortedList.Add(new NoteEvent(60, 1, 1, 100));

            Assert.AreEqual(sortedList[0].Start, 0);
            Assert.AreEqual(sortedList[1].Start, .1m);
            Assert.AreEqual(sortedList[2].Start, .5m);
            Assert.AreEqual(sortedList[3].Start, 1);
            Assert.AreEqual(sortedList[3].Pitch, 60);
            Assert.AreEqual(sortedList[4].Start, 1);
            Assert.AreEqual(sortedList[4].Pitch, 64);
            Assert.AreEqual(sortedList[5].Start, 2);
            Assert.AreEqual(sortedList[6].Start, 2.00001m);
            Assert.AreEqual(sortedList[7].Start, 7);
            Assert.AreEqual(sortedList[8].Start, 10);
            Assert.AreEqual(sortedList[9].Start, 12.5m);
        }

        [Test]
        public void TestDuplicateEntries()
        {
            var list = new SortedList<NoteEvent>();
            var noteEvent = new NoteEvent(64, 1, 1, 100);
            list.Add(noteEvent);
            list.Add(new NoteEvent(noteEvent));
            Assert.AreEqual(list.Count, 1);
        }
    }
}

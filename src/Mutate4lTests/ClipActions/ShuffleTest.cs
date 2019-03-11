using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Commands;
using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Utility;

namespace Mutate4lTests.ClipActions
{
    [TestClass]
    public class ShuffleTest
    {
        [TestMethod]
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

        //[TestMethod]
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

        [TestMethod]
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
}
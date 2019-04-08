using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Commands;
using Mutate4l.Core;

namespace Mutate4lTests.ClipActions
{
    [TestClass]
    public class ConstrainTest
    {
        [TestMethod]
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
            var options = new ConstrainOptions()
            {
                Mode = ConstrainMode.Pitch
            };
            var resultObj = Constrain.Apply(options, clip1, clip2);
            Assert.IsTrue(resultObj.Success);
            var result = resultObj.Result[0];
            Assert.AreEqual(48, result.Notes[0].Pitch);
            Assert.AreEqual(62, result.Notes[1].Pitch);
            Assert.AreEqual(79, result.Notes[2].Pitch);
        }
    }
}
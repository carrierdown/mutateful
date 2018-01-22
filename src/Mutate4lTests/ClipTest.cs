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
//        [TestMethod]
        public void TestChunkifyAndFlatten()
        {
            var clip1 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, 2m, 100),
                   new NoteEvent(62, .5m, .5m, 100),
                   new NoteEvent(62, 1, .5m, 100),
                   new NoteEvent(62, 1.5m, .5m, 100),
                   new NoteEvent(60, 2, .5m, 100),
                   new NoteEvent(62, 2.5m, .5m, 100),
                   new NoteEvent(62, 3, .5m, 100)
                }
            };

//            clip1.Chunkify(clip1.Notes.Except(clip1.Notes[0]).Where(x => x.Start >= clip1.Notes[0]))

        }
    }
}

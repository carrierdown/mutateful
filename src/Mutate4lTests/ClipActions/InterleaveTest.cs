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
        public void TestInterleave()
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
            Interleave.EventRangeA = 0.25m;
            Interleave.EventRangeB = 0.25m;
            var result = Interleave.Apply(clip1, clip2);
            Console.WriteLine("hei");
        }
    }
}

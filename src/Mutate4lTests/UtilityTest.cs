using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Cli;
using Mutate4l.Dto;
using Mutate4l.ClipActions;
using System.Collections.Generic;
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
                Notes = new List<Note>()
                {
                   new Note(60, 0, .25m, 100),
                   new Note(60, .4m, .1m, 100),
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
            var clip3 = new Clip(2, true)
            {
                Notes = new List<Note>()
                {
                   new Note(64, .5m, 1, 100),
                   new Note(64, 1.5m, .5m, 100),
                }
            };
            Utility.NormalizeClipLengths(clip1, clip2, clip3);
            Console.WriteLine();
        }
    }
}

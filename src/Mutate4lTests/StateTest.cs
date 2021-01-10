using System.Linq;
using System.Threading.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Core;
using Mutate4l.IO;
using Mutate4l.State;

namespace Mutate4lTests
{
    [TestClass]
    public class StateTest
    {
        [TestMethod]
        public void TestTwoClipsAndFormula()
        {
            var queue = Channel.CreateUnbounded<InternalCommand>();
            
            var setClipDataHeader = new byte[] {127, 126, 125, 255};
            var setFormulaHeader = new byte[] {127, 126, 125, 254};
            var evaluateFormulasHeader = new byte[] {127, 126, 125, 253}; 

            var dataForFirstClip = new byte[] {1, 1, 0, 0, 128, 64, 1, 1, 0, 60, 0, 0, 0, 0, 0, 0, 128, 64, 101};
            var dataForSecondClip = new byte[] {1, 2, 0, 0, 128, 64, 1, 1, 0, 62, 0, 0, 0, 0, 0, 0, 128, 64, 101};
            var formulaData = new byte[] {1, 3, 65, 49, 32, 65, 50, 32, 105, 108, 32, 45, 115, 107, 105, 112}; // A1 A2 il -skip

            var clipSet = new ClipSet();
            Decoder.HandleTypedCommand(setClipDataHeader.Concat(dataForFirstClip).ToArray(), clipSet, queue.Writer);
            Decoder.HandleTypedCommand(setClipDataHeader.Concat(dataForSecondClip).ToArray(), clipSet, queue.Writer);
            Decoder.HandleTypedCommand(setFormulaHeader.Concat(formulaData).ToArray(), clipSet, queue.Writer);
            Decoder.HandleTypedCommand(evaluateFormulasHeader, clipSet, queue.Writer);
            
            Assert.AreEqual(clipSet[1,1].Clip.Count, 1);
            Assert.AreEqual(clipSet[1,2].Clip.Count, 1);
        }
    }
}
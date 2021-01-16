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
        
        /*
127, 126, 125, 254, 0, 0, 97, 50, 32, 98, 50, 32, 105, 108

127, 126, 125, 255, 0, 1, 0, 0, 128, 64, 1, 4, 0, 66, 0, 0, 0, 0, 0, 0, 128, 63, 100, 66, 0, 0, 0, 64, 0, 0, 128, 63, 100, 70, 0, 0, 128, 63, 0, 0, 128, 63, 100, 70, 0, 0, 64, 64, 0, 0, 128, 63, 100

127, 126, 125, 255, 0, 2, 0, 0, 128, 64, 1, 4, 0, 60, 0, 0, 192, 63, 0, 0, 0, 63, 100, 60, 0, 0, 32, 64, 0, 0, 0, 63, 100, 64, 0, 0, 0, 0, 0, 0, 0, 63, 100, 64, 0, 0, 128, 63, 0, 0, 0, 63, 100

127, 126, 125, 254, 0, 3, 98, 51

127, 126, 125, 255, 1, 0, 0, 0, 128, 64, 1, 4, 0, 60, 0, 0, 0, 64, 0, 0, 0, 63, 100, 60, 0, 0, 64, 64, 0, 0, 0, 63, 100, 64, 0, 0, 0, 63, 0, 0, 0, 63, 100, 64, 0, 0, 192, 63, 0, 0, 0, 63, 100

127, 126, 125, 254, 1, 1, 98, 49, 32, 97, 51, 32, 105, 108

127, 126, 125, 255, 1, 2, 0, 0, 128, 64, 1, 2, 0, 60, 0, 0, 0, 0, 0, 0, 192, 63, 100, 60, 0, 0, 192, 63, 0, 0, 192, 63, 100

127, 126, 125, 254, 1, 3, 97, 51, 32, 97, 52, 32, 99, 97, 116, 32, 115, 104, 117, 102, 102, 108, 101, 32, 45, 98, 121, 32, 98, 50
         */
    }
}
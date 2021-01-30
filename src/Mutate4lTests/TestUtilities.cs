using System.Linq;
using Mutate4l;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.IO;
using Mutate4l.Utility;
using NUnit.Framework;

namespace Mutate4lTests
{
    public static class TestUtilities
    {
        public static void InputShouldProduceGivenOutput(byte[] input, byte[] output)
        {
            var (clips, formula, id, trackNo) = Decoder.DecodeData(input);
            var chainedCommandWrapper = Parser.ParseFormulaToChainedCommand(formula, clips, new ClipMetaData(id, trackNo));
            Assert.IsTrue(chainedCommandWrapper.Success);

            var processedClipWrapper = ClipProcessor.ProcessChainedCommand(chainedCommandWrapper.Result);
            Assert.IsTrue(processedClipWrapper.Success);
            Assert.IsTrue(processedClipWrapper.Result.Length > 0);

            var processedClip = processedClipWrapper.Result[0];
            byte[] clipData = IOUtilities.GetClipAsBytes(chainedCommandWrapper.Result.TargetMetaData.Id, processedClip).ToArray();
            
            Assert.IsTrue(output.Length == clipData.Length);
            Assert.IsTrue(output.SequenceEqual(clipData));
        }
    }
}
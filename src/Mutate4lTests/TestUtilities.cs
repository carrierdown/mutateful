using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mutate4l;
using Mutate4l.Compiler;
using Mutate4l.Core;
using Mutate4l.Utility;
using NUnit.Framework;
using Decoder = Mutate4l.IO.Decoder;

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
        
        public static void PrintSyntaxTree(List<TreeToken> treeTokens, int indent = 0)
        {
            foreach (var treeToken in treeTokens)
            {
                Console.WriteLine($"{GetIndent(indent)}{treeToken.Value}");
                if (treeToken.HasChildren) PrintSyntaxTree(treeToken.Children, indent + 1);
            }
        }

        public static string GetIndent(int indent)
        {
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < indent; i++)
            {
                sb.Append("  ");
            }
            return sb.ToString();
        }
    }
}
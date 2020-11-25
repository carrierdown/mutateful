﻿using Mutate4l.IO;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using Mutate4l.Core;
using Mutate4l.State;

namespace Mutate4l.Cli
{
    internal static class CliHandler
    {
        public const string UnitTestDirective = " test";
        public const string SvgDocDirective = " doc";

        public static ClipSlot HandleInput(byte[] inputData)
        {
            var generateUnitTest = false;
            var generateSvgDoc = false;

            if (Decoder.IsStringData(inputData))
            {
                string text = Decoder.GetText(inputData);
                Console.WriteLine(text);
                return ClipSlot.Empty;
            }

            (List<Clip> clips, string formula, ushort id, byte trackNo) = Decoder.DecodeData(inputData);
            formula = formula.Trim(' ');
            Console.WriteLine($"Received {clips.Count} clips and formula: {formula}");
            if (formula.EndsWith(UnitTestDirective))
            {
                Console.WriteLine(
                    $"Saving autogenerated unit test to {Path.Join(Environment.CurrentDirectory, "GeneratedUnitTests.txt")}");
                formula = formula.Substring(0, formula.Length - UnitTestDirective.Length);
                generateUnitTest = true;
            }

            if (formula.EndsWith(SvgDocDirective))
            {
                Console.WriteLine(
                    $"Saving autogenerated SVG documentation for this formula to {Path.Join(Environment.CurrentDirectory, "GeneratedDocs.svg")}");
                formula = formula.Substring(0, formula.Length - SvgDocDirective.Length);
                generateSvgDoc = true;
            }

            var chainedCommandWrapper = Parser.ParseFormulaToChainedCommand(formula, clips, new ClipMetaData(id, trackNo));
            if (!chainedCommandWrapper.Success)
            {
                Console.WriteLine(chainedCommandWrapper.ErrorMessage);
                return ClipSlot.Empty;
            }

            ProcessResultArray<Clip> processedClipWrapper;
            try
            {
                processedClipWrapper = ClipProcessor.ProcessChainedCommand(chainedCommandWrapper.Result);
            }
            catch (Exception e)
            {
                processedClipWrapper =
                    new ProcessResultArray<Clip>($"{formula}. Please check your syntax. Exception: {e.Message}");
            }

            if (processedClipWrapper.WarningMessage.Length > 0)
            {
                Console.WriteLine($"Warnings were generated:{System.Environment.NewLine}" +
                                  $"{processedClipWrapper.WarningMessage}");
            }

            if (processedClipWrapper.Success && processedClipWrapper.Result.Length > 0)
            {
                var processedClip = processedClipWrapper.Result[0];
                byte[] processedClipData = IOUtilities.GetClipAsBytes(chainedCommandWrapper.Result.TargetMetaData.Id, processedClip).ToArray();

                if (generateUnitTest)
                {
                    TestUtilities.AppendUnitTest(formula, inputData, processedClipData);
                }

                if (generateSvgDoc)
                {
                    SvgUtilities.GenerateSvgDoc(formula, clips, processedClip, 882, 300);
                }
                ClipSlot processedClipSlot = new ClipSlot(formula, processedClip, chainedCommandWrapper.Result, id);
                return processedClipSlot;
            }
            Console.WriteLine($"Error applying formula: {processedClipWrapper.ErrorMessage}");
            return ClipSlot.Empty;
        }

        public static void Start()
        {
            /*
            while (true)
            {
                var generateUnitTest = false;
                var generateSvgDoc = false;
                var inputData = UdpConnector.WaitForData();
                
                if (UdpConnector.IsString(inputData))
                {
                    string text = UdpConnector.GetText(inputData);
                    Console.WriteLine(text);
                    continue;
                }

                (List<Clip> clips, string formula, ushort id, byte trackNo) = UdpConnector.DecodeData(inputData);
                Console.WriteLine($"Received {clips.Count} clips and formula: {formula}");
                if (formula.EndsWith(UnitTestDirective))
                {
                    Console.WriteLine(
                        $"Saving autogenerated unit test to {Path.Join(Environment.CurrentDirectory, "GeneratedUnitTests.txt")}");
                    formula = formula.Substring(0, formula.Length - UnitTestDirective.Length);
                    generateUnitTest = true;
                }

                if (formula.EndsWith(SvgDocDirective))
                {
                    Console.WriteLine(
                        $"Saving autogenerated SVG documentation for this formula to {Path.Join(Environment.CurrentDirectory, "GeneratedDocs.svg")}");
                    formula = formula.Substring(0, formula.Length - SvgDocDirective.Length);
                    generateSvgDoc = true;
                }

                var chainedCommandWrapper = Parser.ParseFormulaToChainedCommand(formula, clips, new ClipMetaData(id, trackNo));
                if (!chainedCommandWrapper.Success)
                {
                    Console.WriteLine(chainedCommandWrapper.ErrorMessage);
                    continue;
                }

                ProcessResultArray<Clip> processedClipWrapper;
                try
                {
                    processedClipWrapper = ClipProcessor.ProcessChainedCommand(chainedCommandWrapper.Result);
                }
                catch (Exception e)
                {
                    processedClipWrapper = new ProcessResultArray<Clip>($"{formula}. Please check your syntax. Exception: {e.Message}");
                }

                if (processedClipWrapper.WarningMessage.Length > 0)
                {
                    Console.WriteLine($"Warnings were generated:{System.Environment.NewLine}" +
                                      $"{processedClipWrapper.WarningMessage}");
                }

                if (processedClipWrapper.Success && processedClipWrapper.Result.Length > 0)
                {
                    var processedClip = processedClipWrapper.Result[0];
                    byte[] processedClipData = IOUtilities.GetClipAsBytes(chainedCommandWrapper.Result.TargetMetaData.Id, processedClip)
                        .ToArray();
                    if (generateUnitTest)
                    {
                        TestUtilities.AppendUnitTest(formula, inputData, processedClipData);
                    }

                    if (generateSvgDoc)
                    {
                        SvgUtilities.GenerateSvgDoc(formula, clips, processedClip, 882, 300);
                    }

                    UdpConnector.SetClipAsBytesById(processedClipData);
                }
                else
                    Console.WriteLine($"Error applying formula: {processedClipWrapper.ErrorMessage}");
            }
        */
        }
    }
}
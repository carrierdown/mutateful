using Mutateful.Commands;
using Mutateful.Compiler;
using Mutateful.Core;
using NUnit.Framework;

namespace MutatefulTests;

[TestFixture]
public class ParserTest
{
    private Clip Clip1 = new Clip(4, true)
    {
        Notes = new SortedList<NoteEvent>()
        {
            new NoteEvent(60, 0, .5m, 100), // C
            new NoteEvent(55, 1, .5m, 100), // G
            new NoteEvent(62, 2, .5m, 100) // D
        }
    };

    private Clip Clip2 = new Clip(4, true)
    {
        Notes = new SortedList<NoteEvent>()
        {
            new NoteEvent(47, 0, .5m, 100), // B
            new NoteEvent(63, 3, .5m, 100), // D#
            new NoteEvent(81, 4, .5m, 100) // A
        }
    };

    [Test]
    public void TestResolveClipReference()
    {
        Tuple<int, int> result = Parser.ResolveClipReference("B4");
        Assert.AreEqual(result.Item1, 1);
        Assert.AreEqual(result.Item2, 3);
    }

    [Test]
    public void TestParseInvalidInput()
    {
        var result = Parser.ParseFormulaToChainedCommand("[0] interleave % fisk -by [1] -mode event",
            new List<Clip> { Clip1, Clip2 }, new ClipMetaData(100, 0));
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void TestParseInvalidInput2()
    {
        var result = Parser.ParseFormulaToChainedCommand("[0] slice /16", new List<Clip> { Clip1, Clip2 },
            new ClipMetaData(100, 0));
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void TestParseInvalidInputDoubleParam()
    {
        var result = Parser.ParseFormulaToChainedCommand("[0] arp -by [1] -by [2]",
            new List<Clip> { Clip1, Clip2, Clip1 }, new ClipMetaData(100, 0));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void TestParseMusicalDivision()
    {
        var result = Parser.ParseFormulaToChainedCommand("[0] interleave -mode time -ranges 1/16 15/16 16/1",
            new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void TestCastNumberToMusicalDivision()
    {
        var command = Parser.ParseFormulaToChainedCommand("[0] interleave -mode time -ranges 1/8 2 1 4",
            new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
        Assert.IsTrue(command.Success);
        var (success, _) = OptionParser.TryParseOptions(command.Result.Commands.First(), out InterleaveOptions options);
        Assert.IsTrue(success);
        Assert.AreEqual(0.5m, options.Ranges[0]);
        Assert.AreEqual(8, options.Ranges[1]);
        Assert.AreEqual(4, options.Ranges[2]);
        Assert.AreEqual(16, options.Ranges[3]);
    }

    [Test]
    public void TestConvertBarsBeatsSixteenths()
    {
        var command = Parser.ParseFormulaToChainedCommand(
            "[0] interleave -mode time -ranges 0.0.2 0.2.0 0.1.0 1.0.0 1.0.1", new List<Clip> { Clip1 },
            new ClipMetaData(100, 0));
        Assert.IsTrue(command.Success);
        var (success, _) = OptionParser.TryParseOptions(command.Result.Commands.First(), out InterleaveOptions options);
        Assert.IsTrue(success);
        Assert.AreEqual(0.5m, options.Ranges[0]);
        Assert.AreEqual(2, options.Ranges[1]);
        Assert.AreEqual(1, options.Ranges[2]);
        Assert.AreEqual(4, options.Ranges[3]);
        Assert.AreEqual(4.25m, options.Ranges[4]);
    }

    [Test]
    public void TestCastNumberWhenNoImplicitCastSet()
    {
        var command =
            Parser.ParseFormulaToChainedCommand("[0] resize 1/8", new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
        Assert.IsTrue(command.Success);
        var (success, _) = OptionParser.TryParseOptions(command.Result.Commands.First(), out ResizeOptions _);
        Assert.IsFalse(success);
        command = Parser.ParseFormulaToChainedCommand("[0] resize 0.5", new List<Clip> { Clip1 },
            new ClipMetaData(100, 0));
        (success, _) = OptionParser.TryParseOptions(command.Result.Commands.First(), out ResizeOptions _);
        Assert.IsTrue(success);
    }

    [Test]
    public void ShowGeneratedSyntaxTree()
    {
        var lexer = new Lexer("[0] tp 2 remap -to [0] shuffle 1 2|3|9x6|5|4 tp 12",
            new List<Clip> { Clip.Empty, Clip.Empty });
        var result = lexer.GetTokens();
        Assert.IsTrue(result.Success);
        var sTokens = Parser.CreateSyntaxTree(result.Result);
        TestUtilities.PrintSyntaxTree(sTokens.Result.Children);
    }

    [Test]
    public void TestParseNestedOperators()
    {
        var command = Parser.ParseFormulaToChainedCommand("[0] shuffle 1 2|3|9x6 1 2 4x3 5|6|7 8",
            new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
        Assert.IsTrue(command.Success);
        Assert.AreEqual(1, command.Result.Commands.Count);

        var flattenedValues = command.Result.Commands[0].DefaultOptionValues.Select(x => int.Parse(x.Value)).ToList();
        var expectedValues = new List<int>
            { 1, 2, 1, 2, 4, 4, 4, 5, 8, 1, 3, 1, 2, 4, 4, 4, 6, 8, 1, 9, 9, 9, 9, 9, 9, 1, 2, 4, 4, 4, 7, 8 };
        Assert.IsTrue(flattenedValues.SequenceEqual(expectedValues));

        command = Parser.ParseFormulaToChainedCommand("[0] loop 4 rat 2 3|4|5x2 6", new List<Clip> { Clip1 },
            new ClipMetaData(100, 0));
        Assert.IsTrue(command.Success);
    }

    [Test]
    public void TestParseNestedCommands()
    {
        var lexer = new Lexer("[0] (([0] transpose 12)([0] transpose 24) il) il", new List<Clip> { Clip.Empty });
        var result = lexer.GetTokens();
        Assert.IsTrue(result.Success);
        var (success, errorMessage) = Parser.AreTokensValid(result.Result);
        if (!success) Console.WriteLine($"Failed with: {errorMessage}");
        Assert.IsTrue(success);
        var sTokens = Parser.CreateSyntaxTree(result.Result);
        TestUtilities.PrintSyntaxTree(sTokens.Result.Children);
        /*var clip1 = new Clip(4, true)
        {
            Notes = new SortedList<NoteEvent>
            {
                new (60, 0, .5m, 100)
            }
        };
        
        var command = Parser.ParseFormulaToChainedCommand("[0] ([0] transpose 12) il", new List<Clip> { Clip1 }, new ClipMetaData(100, 0));
        Assert.IsTrue(command.Success);*/
    }
}
using Mutateful.Core;
using Mutateful.IO;
using NUnit.Framework;

namespace MutatefulTests;

[TestFixture]
public class CommandHandlerTests
{
    [Test]
    public void TestEvaluationOfClipIsAbortedWhenContentsUnchanged()
    {
        var commandHandler = new CommandHandler();
        var result = commandHandler.SetAndEvaluateClipData(new Clip(4, true)
        {
            ClipReference = ClipReference.FromString("A1"), 
            Notes = new SortedList<NoteEvent> {
                new NoteEvent(36, 0, .5m, 100),
                new NoteEvent(38, 1, .5m, 100),
                new NoteEvent(40, 2, .75m, 100)
            }
        });
        Assert.IsTrue(result.RanToCompletion);
        
        result = commandHandler.SetAndEvaluateClipData(new Clip(4, true)
        {
            ClipReference = ClipReference.FromString("A1"), 
            Notes = new SortedList<NoteEvent> {
                new NoteEvent(36, 0, .75m, 100),
                new NoteEvent(38, 1, .5m, 100),
                new NoteEvent(40, 2, .5m, 100)
            }
        });
        Assert.IsTrue(result.RanToCompletion);
        
        result = commandHandler.SetAndEvaluateClipData(new Clip(4, true)
        {
            ClipReference = ClipReference.FromString("A1"), 
            Notes = new SortedList<NoteEvent> {
                new NoteEvent(36, 0, .75m, 100),
                new NoteEvent(38, 1, .5m, 100),
                new NoteEvent(40, 2, .5m, 100)
            }
        });
        Assert.IsFalse(result.RanToCompletion);
        Assert.AreEqual($"Aborted evaluation of clip at A1 since it was unchanged.", result.Warnings.FirstOrDefault());
    }

    [Test]
    public void TestEvaluationOfFormulaIsAbortedWhenContentsUnchanged()
    {
        var commandHandler = new CommandHandler();
        var result = commandHandler.SetAndEvaluateFormula("a1 slice 1/8 ratchet 4 8 12 2 1 6", 1, 1);
        Assert.IsTrue(result.RanToCompletion);
        result = commandHandler.SetAndEvaluateFormula("a1 slice 1/8 ratchet 4 8 12 2 1 5", 1, 1);
        Assert.IsTrue(result.RanToCompletion);
        result = commandHandler.SetAndEvaluateFormula("a1 slice 1/8 ratchet 4 8 12 2 1 5", 1, 1);
        Assert.IsFalse(result.RanToCompletion);
    }
}
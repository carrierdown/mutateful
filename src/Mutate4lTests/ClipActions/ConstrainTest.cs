using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.Commands;
using Mutate4l.Core;
using Mutate4l.Dto;

namespace Mutate4lTests.ClipActions
{
    [TestClass]
    public class ConstrainTest
    {
        [TestMethod]
        public void TestConstrainNoteEventPitch()
        {
            var clip1 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                   new NoteEvent(60, 0, .5m, 100),
                   new NoteEvent(55, 1, .5m, 100),
                   new NoteEvent(62, 2, .5m, 100)
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new SortedList<NoteEvent>()
                {
                    new NoteEvent(47, 0, .5m, 100),
                    new NoteEvent(63, 3, .5m, 100),
                    new NoteEvent(81, 4, .5m, 100)
                }
            };
            var options = new ConstrainOptions()
            {
                Mode = ConstrainMode.Pitch
            };
            var resultObj = Constrain.Apply(options, clip1, clip2);
            Assert.IsTrue(resultObj.Success);
            var result = resultObj.Result[0];
            Assert.AreEqual(48, result.Notes[0].Pitch);
            Assert.AreEqual(62, result.Notes[1].Pitch);
            Assert.AreEqual(79, result.Notes[2].Pitch);
        }
        /*       [TestMethod]
               public void MyTestMethod()
               {
                   var lexer = new Lexer("interleave a1 b1 mode time ranges 1/2 1/4 counts 1 2 => a2");
                   Command structuredCommand = Parser.ParseTokensToCommand(lexer.GetTokens());
                   InterleaveOptions interleaveOptions = OptionParser.ParseOptions<InterleaveOptions>(structuredCommand.Options);
                   Console.Write("");
               }*/
    }
}


/*
tape('Constrain NoteEvent Start', (test) => {
    var notesToMutate = [
        new NoteEvent(36, "0.25", "0.25", 100, 0),
        new NoteEvent(36, "1", "0.5", 100, 0),
        new NoteEvent(36, "4", "0.25", 100, 0),
        new NoteEvent(36, "3.5", "0.25", 100, 0)
    ];
    var notesToSourceFrom = [
        new NoteEvent(36, "0.5", "0.25", 100, 0),
        new NoteEvent(36, "3", "0.5", 100, 0),
        new NoteEvent(36, "4", "0.25", 100, 0),
        new NoteEvent(36, "5", "0.25", 100, 0)
    ];
    var dst = { getNoteEvents: () => { return notesToMutate; }, getLength: () => { return new Big(4); } };
    var src = { getNoteEvents: () => { return notesToSourceFrom; }, getLength: () => { return new Big(4); } };

    var clipActions = new ClipActions();
    var resultClip = clipActions.process(Action.Constrain, dst, src, {
        constrainNoteEventStart: true,
        constrainNoteEventPitch: false
    });
    var results = resultClip.notes;

    test.equal(results[0].getStartAsString(), "0.5000", `NoteEvent start values should be equal. Actual: ${results[0].getStartAsString()} Expected: 0.5000`);
    test.equal(results[1].getStartAsString(), "0.5000", `NoteEvent start values should be equal. Actual: ${results[1].getStartAsString()} Expected: 0.5000`);
    test.equal(results[2].getStartAsString(), "4.0000", `NoteEvent start values should be equal. Actual: ${results[2].getStartAsString()} Expected: 4.0000`);
    test.equal(results[3].getStartAsString(), "3.0000", `NoteEvent start values should be equal. Actual: ${results[3].getStartAsString()} Expected: 3.0000`);

    test.end();
});

tape('Constrain NoteEvent Pitch', (test) => {
    var notesToMutate = [
        new NoteEvent(35, "0.25", "0.25", 100, 0),
        new NoteEvent(38, "1", "0.5", 100, 0),
        new NoteEvent(41, "4", "0.25", 100, 0),
        new NoteEvent(30, "3.5", "0.25", 100, 0)
    ];
    var notesToSourceFrom = [
        new NoteEvent(36, "0.5", "0.25", 100, 0),
        new NoteEvent(40, "3", "0.5", 100, 0),
        new NoteEvent(34, "4", "0.25", 100, 0),
        new NoteEvent(39, "5", "0.25", 100, 0)
    ];
    var dst = { getNoteEvents: () => { return notesToMutate; }, getLength: () => { return new Big(4); } };
    var src = { getNoteEvents: () => { return notesToSourceFrom; }, getLength: () => { return new Big(4); } };

    var clipActions = new ClipActions();
    var resultClip = clipActions.process(Action.Constrain, dst, src, {
        constrainNoteEventStart: false,
        constrainNoteEventPitch: true
    });
    var results = resultClip.notes;

    test.equal(results[0].getPitch(), 36, `Pitch values should be equal. Actual: ${results[0].getPitch()} Expected: 36`);
    test.equal(results[1].getPitch(), 39, `Pitch values should be equal. Actual: ${results[1].getPitch()} Expected: 39`);
    test.equal(results[2].getPitch(), 40, `Pitch values should be equal. Actual: ${results[2].getPitch()} Expected: 40`);
    test.equal(results[3].getPitch(), 34, `Pitch values should be equal. Actual: ${results[3].getPitch()} Expected: 34`);

    test.end();
});
 * 
 * * */

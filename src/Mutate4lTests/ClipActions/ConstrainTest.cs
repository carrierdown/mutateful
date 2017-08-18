using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mutate4l.ClipActions;
using Mutate4l.Core;
using Mutate4l.Cli;
using Mutate4l.Dto;
using System;
using System.Collections.Generic;

namespace Mutate4lTests.ClipActions
{
    [TestClass]
    public class ConstrainTest
    {
        //[TestMethod]
        public void TestConstrainNoteStart()
        {
            /*var clip1 = new Clip(4, true)
            {
                Notes = new SortedList<Note>()
                {
                   new Note(36, .5m, .25m, 100),
                   new Note(36, 3, .5m, 100),
                   new Note(36, 4, .25m, 100),
                   new Note(36, 5, .25m, 100)
                }
            };
            var clip2 = new Clip(4, true)
            {
                Notes = new SortedList<Note>()
                {
                    new Note(36, .5m, .25m, 100),
                    new Note(36, 3, .5m, 100),
                    new Note(36, 4, .25m, 100),
                    new Note(36, 5, .25m, 100)
                }
            };
            var options = new ConstrainOptions()
            {
                Start = true
            };
            var resultObj = Constrain.Apply(options, clip1, clip2);*/
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
tape('Constrain Note Start', (test) => {
    var notesToMutate = [
        new Note(36, "0.25", "0.25", 100, 0),
        new Note(36, "1", "0.5", 100, 0),
        new Note(36, "4", "0.25", 100, 0),
        new Note(36, "3.5", "0.25", 100, 0)
    ];
    var notesToSourceFrom = [
        new Note(36, "0.5", "0.25", 100, 0),
        new Note(36, "3", "0.5", 100, 0),
        new Note(36, "4", "0.25", 100, 0),
        new Note(36, "5", "0.25", 100, 0)
    ];
    var dst = { getNotes: () => { return notesToMutate; }, getLength: () => { return new Big(4); } };
    var src = { getNotes: () => { return notesToSourceFrom; }, getLength: () => { return new Big(4); } };

    var clipActions = new ClipActions();
    var resultClip = clipActions.process(Action.Constrain, dst, src, {
        constrainNoteStart: true,
        constrainNotePitch: false
    });
    var results = resultClip.notes;

    test.equal(results[0].getStartAsString(), "0.5000", `Note start values should be equal. Actual: ${results[0].getStartAsString()} Expected: 0.5000`);
    test.equal(results[1].getStartAsString(), "0.5000", `Note start values should be equal. Actual: ${results[1].getStartAsString()} Expected: 0.5000`);
    test.equal(results[2].getStartAsString(), "4.0000", `Note start values should be equal. Actual: ${results[2].getStartAsString()} Expected: 4.0000`);
    test.equal(results[3].getStartAsString(), "3.0000", `Note start values should be equal. Actual: ${results[3].getStartAsString()} Expected: 3.0000`);

    test.end();
});

tape('Constrain Note Pitch', (test) => {
    var notesToMutate = [
        new Note(35, "0.25", "0.25", 100, 0),
        new Note(38, "1", "0.5", 100, 0),
        new Note(41, "4", "0.25", 100, 0),
        new Note(30, "3.5", "0.25", 100, 0)
    ];
    var notesToSourceFrom = [
        new Note(36, "0.5", "0.25", 100, 0),
        new Note(40, "3", "0.5", 100, 0),
        new Note(34, "4", "0.25", 100, 0),
        new Note(39, "5", "0.25", 100, 0)
    ];
    var dst = { getNotes: () => { return notesToMutate; }, getLength: () => { return new Big(4); } };
    var src = { getNotes: () => { return notesToSourceFrom; }, getLength: () => { return new Big(4); } };

    var clipActions = new ClipActions();
    var resultClip = clipActions.process(Action.Constrain, dst, src, {
        constrainNoteStart: false,
        constrainNotePitch: true
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

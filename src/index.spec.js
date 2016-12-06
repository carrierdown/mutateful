const tape = require('tape');

tape('Constrain Note Start', (test) => {
    // quick and dirty "mocks"...
    var src = {
        getNotes: () => {
            return [
                new Note(36, "0.25", "0.25", 100, 0),
                new Note(36, "1", "0.5", 100, 0),
                new Note(36, "4", "0.25", 100, 0),
                new Note(36, "3.5", "0.25", 100, 0)
            ];
        }
    };
    var dst = {
        getNotes: () => {
            return [
                new Note(36, "0.5", "0.25", 100, 0),
                new Note(36, "3", "0.5", 100, 0),
                new Note(36, "4", "0.25", 100, 0),
                new Note(36, "5", "0.25", 100, 0)
            ];
        }
    };
    var result = applyConstrainNoteStart(src, dst);
    test.equal(result[0].getStartAsString(), "0.5000", `Note start values should be equal. Actual: ${result[0].getStartAsString()} Expected: 0.5000`);
    test.equal(result[1].getStartAsString(), "0.5000", `Note start values should be equal. Actual: ${result[1].getStartAsString()} Expected: 0.5000`);
    test.equal(result[2].getStartAsString(), "4.0000", `Note start values should be equal. Actual: ${result[2].getStartAsString()} Expected: 4.0000`);
    test.equal(result[3].getStartAsString(), "3.0000", `Note start values should be equal. Actual: ${result[3].getStartAsString()} Expected: 3.0000`);

    test.end();
});

tape('Constrain Note Pitch', (test) => {
    // quick and dirty "mocks"...
    var src = {
        getNotes: () => {
            return [
                new Note(35, "0.25", "0.25", 100, 0),
                new Note(38, "1", "0.5", 100, 0),
                new Note(41, "4", "0.25", 100, 0),
                new Note(30, "3.5", "0.25", 100, 0)
            ];
        }
    };
    var dst = {
        getNotes: () => {
            return [
                new Note(36, "0.5", "0.25", 100, 0),
                new Note(40, "3", "0.5", 100, 0),
                new Note(34, "4", "0.25", 100, 0),
                new Note(39, "5", "0.25", 100, 0)
            ];
        }
    };
    // console.log(src.getNotes()[0].toString(), dst.getNotes()[0].toString());
    var result = applyConstrainNotePitch(src, dst);
    test.equal(result[0].getPitch(), 36, `Pitch values should be equal. Actual: ${result[0].getPitch()} Expected: 36`);
    test.equal(result[1].getPitch(), 39, `Pitch values should be equal. Actual: ${result[1].getPitch()} Expected: 39`);
    test.equal(result[2].getPitch(), 40, `Pitch values should be equal. Actual: ${result[2].getPitch()} Expected: 40`);
    test.equal(result[3].getPitch(), 34, `Pitch values should be equal. Actual: ${result[3].getPitch()} Expected: 34`);

    test.end();
});

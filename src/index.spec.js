const tape = require('tape');

tape('Testing compiler with valid code', (test) => {
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
    console.log(src.getNotes()[0].toString(), dst.getNotes()[0].toString());
    var result = doConstrainStart(src, dst);
    test.equal(result[0].getStartAsString(), "0.5000", "Values should be equal");
    test.equal(result[1].getStartAsString(), "0.5000", "Values should be equal");
    test.equal(result[2].getStartAsString(), "4.0000", "Values should be equal");
    test.equal(result[3].getStartAsString(), "3.0000", "Values should be equal");

    test.end();
});

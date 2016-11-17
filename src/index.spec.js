const tape = require('tape');

tape('Testing compiler with valid code', (test) => {
    // quick and dirty "mocks"...
    var src = {
        getNotes: () => {
            return [new Note(36, "0.25", "0.25", 100, 0)];
        }
    };
    var dst = {
        getNotes: () => {
            return [new Note(36, "0.50", "0.25", 100, 0)];
        }
    };
    console.log(src.getNotes()[0].toString(), dst.getNotes()[0].toString());
    var result = doConstrainStart(src, dst);
    console.log(src.getNotes()[0].toString(), result);
    // clip.setNotes([new Note(48, "2", ".25", 100, 0)]);

    var a = BigFactory.create(0.03);
    var b = BigFactory.create(0.7);

    console.log(a.toFixed(4));
    test.equal(1, 1, "tests are up");
    test.equal(b.lt(a), false, "should work");

    test.end();
});

const tape = require('tape');

tape('Testing compiler with valid code', (test) => {
    // var clip = new Clip();
    // clip.setNotes([new Note(48, "2", ".25", 100, 0)]);

    var a = BigFactory.create(0.03);
    var b = BigFactory.create(0.7);

    console.log(a.toFixed(4));
    test.equal(1, 1, "tests are up");
    test.equal(b.lt(a), false, "should work");

    test.end();
});

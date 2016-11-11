///<reference path="big-def.ts"/>

var a: IBig = BigFactory.create(1.5);
var b: IBig = BigFactory.create(0.7);

console.log(a.toFixed(4));
console.log(b.lt(a));
console.log(a.lt(b));

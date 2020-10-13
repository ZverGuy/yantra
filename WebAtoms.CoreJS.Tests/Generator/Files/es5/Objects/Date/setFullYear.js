﻿let event = new Date('August 19, 1975 23:15:30');

event.setFullYear(1969);

assert.strictEqual(1969, event.getFullYear());
// expected output: 1969

event.setFullYear(1);
assert.strictEqual(1, event.getFullYear()); // expected output: 1

assert(isNaN(event.setFullYear(0)));
//event.setFullYear(0); //edge case, cannot be tested in .net, but we have set it to NaN

event = new Date('August 19, 1975 23:15:30');
assert(isNaN(event.setFullYear(-1)));


event = new Date('August 19, 1975 23:15:30');
assert(isNaN(event.setFullYear(NaN)));
assert.strictEqual(3, event.setFullYear.length);

event = new Date('August 19, 1975 23:15:30');
assert.strictEqual(240342330000, event.setFullYear(1975, 30, 44)); 
event = new Date('August 19, 1975 23:15:30');
event.setFullYear(2020, -20, -50);
//console.log(event);
assert.strictEqual(1520790330000, event.getTime()); 

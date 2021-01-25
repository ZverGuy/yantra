﻿function a(b = 2) {
    return b;
}

assert.strictEqual(a(), 2);
assert.strictEqual(a(1), 1);

var a = {};
a[a["A"] = 1] = "A";

assert.strictEqual(a[1], "A");
assert.strictEqual(a["A"], 1);
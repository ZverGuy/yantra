﻿var a = {};

var b = 1;

function add() {
    b++;
    return b;
}

assert.strictEqual(undefined, a?.call?.(add()));
assert.strictEqual(1, b);

assert.strictEqual(undefined, a.call?.(add()));
assert.strictEqual(1, b);
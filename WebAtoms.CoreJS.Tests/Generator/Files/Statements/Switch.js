﻿var a = "a";
function run() {
    switch (a) {
        case "a":
            a = "b";
            break;
        case "b":
            a = "a";
            break;
        default:
            a = "1";
            break;
    }
}
run();
run();
assert(a === "a");
a = "2";
run();
assert(a === "1");

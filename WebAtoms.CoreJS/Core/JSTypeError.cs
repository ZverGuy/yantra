﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WebAtoms.CoreJS.Core
{
    public class JSTypeError: JSError
    {

        public static readonly KeyString KeyTypeError = "TypeError";

        public static string NotIterable(object name) => $"{name} is not iterable";

        public static string NotEntry(object name) => $"Iterator value {name} is an entry object";

        internal JSTypeError(JSValue message, JSValue stack) : base(message, stack, JSContext.Current.TypeErrorPrototype)
        {
        }

        public new static JSFunction Create()
        {
            var r = new JSFunction((t, a) => new JSTypeError(a[0], JSUndefined.Value));
            return r;
        }
    }
}
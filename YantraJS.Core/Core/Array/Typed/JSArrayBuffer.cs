﻿using System;

namespace YantraJS.Core.Typed
{
    //public abstract class TypedArray<T> : JSValue
    //{
    //    private readonly int length;
    //    private readonly T[] value;

    //    public TypedArray(int length, JSObject prototype) : base(prototype)
    //    {
    //        this.length = length;
    //        this.value = new T[length];
    //    }

    //    public override JSValue this[uint key] { 
    //        get => base[key]; 
    //        set => base[key] = value; 
    //    }
    //}

    public class JSArrayBuffer : JSObject
    {
        internal readonly byte[] buffer;

        public JSArrayBuffer(int length) : base(JSContext.Current.ArrayBufferPrototype)
        {
            this.buffer = new byte[length];
        }

        public override bool BooleanValue => true;

        public override double DoubleValue => Double.NaN;

        public override JSBoolean Equals(JSValue value)
        {
            if (Object.ReferenceEquals(this, value))
                return JSBoolean.True;
            return JSBoolean.False; 
        }

        public override JSValue InvokeFunction(in Arguments a)
        {
            throw JSContext.Current.NewTypeError($"{this} is not a function");
        }

        public override JSBoolean StrictEquals(JSValue value)
        {
            if (Object.ReferenceEquals(this, value))
                return JSBoolean.True;
            return JSBoolean.False;
        }

        [Constructor]
        public static JSValue Constructor(in Arguments a) {
            int length = a.Get1().AsInt32OrDefault();
            if (length < 0 || length > JSNumber.MaxSafeInteger)
            {
                throw JSContext.Current.NewRangeError("Buffer length out of range");
            }
            return new JSArrayBuffer(length);
        }

    }
}
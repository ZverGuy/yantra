﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using WebAtoms.CoreJS.Extensions;
using WebAtoms.CoreJS.Utils;

namespace WebAtoms.CoreJS.Core
{
    [JSRuntime(typeof(JSStringStatic), typeof(JSStringPrototype))]
    public partial class JSString : JSPrimitive
    {

        internal static JSString Empty = new JSString(string.Empty);

        internal readonly string value;
        KeyString _keyString = new KeyString(null,0);

        public override double DoubleValue => NumberParser.CoerceToNumber(value);

        public override bool BooleanValue => value.Length > 0;

        public override long BigIntValue => long.TryParse(value, out var n) ? n : 0;

        public override bool IsString => true;

        internal override KeyString ToKey(bool create = true)
        {
            if (!create)
            {
                if(!KeyStrings.TryGet(this.value, out _keyString))
                    return KeyStrings.undefined;
                return _keyString;
            }
            return _keyString.Value != null
                ? _keyString
                : (_keyString = KeyStrings.GetOrCreate(this.value));
        }

        protected override JSObject GetPrototype()
        {
            return JSContext.Current.StringPrototype;
        }

        public JSString(string value): base()
        {
            this.value = value;
        }

        public JSString(char ch) : this(new string(ch,1))
        {
            
        }


        public JSString(string value, KeyString keyString) : this(value)
        {
            this._keyString = keyString;
        }

        public static implicit operator KeyString(JSString value)
        {
            return value.ToString();
        }

        public override JSValue TypeOf()
        {
            return JSConstants.String;
        }


        public override string ToString()
        {
            return value;
        }

        public override string ToDetailString()
        {
            return value;
        }

        public override JSValue this[uint key] { 
            get
            {
                if (key >= this.value.Length)
                    return JSUndefined.Value;
                return new JSString(new string(this.value[(int)key],1));
            }
            set { } 
        }

        public override int Length => value.Length;

        public override bool Equals(object obj)
        {
            if (obj is JSString v)
                return this.value == v.value;
            return base.Equals(obj);
        }

        public override JSBoolean Equals(JSValue value)
        {
            if (object.ReferenceEquals(this, value))
                return JSBoolean.True;
            switch (value)
            {
                case JSString strValue
                    when ((this.value == strValue.value)
                    || (this.DoubleValue == value.DoubleValue)):
                    return JSBoolean.True;
                case JSNumber number
                    when ((this.DoubleValue == number.value)
                        || (this.value.CompareTo(number.value.ToString()) == 0)):
                    return JSBoolean.True;
                case JSBoolean boolean
                    when (this.DoubleValue == (boolean._value ? 1D : 0D)):
                    return JSBoolean.True;
            }
            return JSBoolean.False;
        }

        public override JSBoolean StrictEquals(JSValue value)
        {
            if (object.ReferenceEquals(this, value))
                return JSBoolean.True;
            if (value is JSString s)
                if (s.value == this.value)
                    return JSBoolean.True;
            return JSBoolean.False;
        }

        public override JSValue InvokeFunction(in Arguments a)
        {
            throw new NotImplementedException($"\"{value}\" is not a function");
        }

        internal override JSBoolean Is(JSValue value)
        {
            if (value is JSString @string && this.value == @string.value)
                return JSBoolean.True;
            return JSBoolean.False;

        }

        internal override IElementEnumerator GetElementEnumerator()
        {
            return new ElementEnumerator(this.value);
        }

        private struct ElementEnumerator : IElementEnumerator
        {

            readonly CharEnumerator en;
            int index;
            public ElementEnumerator(string value)
            {
                this.en = value.GetEnumerator();
                index = -1;
            }

            public bool MoveNext(out bool hasValue, out JSValue value, out uint i)
            {
                if (en.MoveNext())
                {
                    index++;
                    i = (uint)index;
                    hasValue = true;
                    value = new JSString(new string(en.Current, 1));
                    return true;
                }
                i = 0;
                value = JSUndefined.Value;
                hasValue = false;
                return false;
            }

        }

    }
}
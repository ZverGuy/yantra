﻿using System;
using System.Runtime.CompilerServices;

namespace YantraJS.Core
{
    

    public struct KeyString
    {

        enum KeyType
        {
            Empty = 0,
            UInt = 1,
            String = 2,
            Symbol = 3
        }

        public readonly static KeyString Empty = new KeyString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator KeyString(string value)
        {
            return KeyStrings.GetOrCreate(value);
        }

        private readonly KeyType Type;
        public readonly string Value;
        public readonly uint Key;
        public JSValue JSValue;

        public bool HasValue
        {
            get
            {
                return Type != KeyType.Empty;
            }
        }

        public bool IsSymbol
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Type == KeyType.Symbol;
            }
        }

        public bool IsString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Type == KeyType.String;
            }
        }

        public bool IsUInt
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Type == KeyType.UInt;
            }
        }

        internal KeyString(uint key)
        {
            Type = KeyType.UInt;
            this.Value = null;
            this.Key = key;
            this.JSValue = null;
        }


        internal KeyString(string value, uint key)
        {
            Type = KeyType.String;
            this.Value = value;
            this.Key = key;
            this.JSValue = null;
        }


        internal KeyString(string value, uint key, JSString @string)
        {
            Type = KeyType.String;
            this.Value = value;
            this.Key = key;
            this.JSValue = @string;
        }

        internal KeyString(string value, uint key, JSSymbol symbol)
        {
            Type = KeyType.String;
            this.Value = value;
            this.Key = key;
            this.JSValue = symbol;
        }

        public override bool Equals(object obj)
        {
            if (obj is KeyString k)
                return Key == k.Key && Type == k.Type && JSValue == k.JSValue;
            if (obj is string sv)
                return Value == sv;
            return false;
        }

        public override int GetHashCode()
        {
            return (int)Key;
        }

        public override string ToString()
        {
            return Value;
        }

        public JSValue ToJSValue()
        {
            if (JSValue != null)
                return JSValue;
            return (JSValue = new JSString(Value, this));
        }

        public static (int size, int total, int next) Total =>
            KeyStrings.Total;

    }
}
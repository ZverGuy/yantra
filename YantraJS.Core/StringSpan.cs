﻿#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using YantraJS.Internals;

namespace YantraJS.Core
{

    public static class StringSpanExtensions
    {
        public static StringSpan ToStringSpan(this string text, int offset, int length) => new StringSpan(text, offset, length);

        public unsafe static void Append(this StringBuilder sb , in StringSpan span)
        {
            fixed (char* start = span.Source)
            {
                char* ch1 = start + (span.Offset);
                sb.Append(ch1, span.Length);
            }
        }
    }

    [DebuggerDisplay("{Key}: {Value}")]
    public struct KeyValue
    {
        public string Key;
        public JSValue Value;
    }

    [DebuggerDisplay("{Value}")]
    public readonly struct StringSpan: 
        IEquatable<StringSpan>, 
        IEquatable<string>,
        IEnumerable<char>
    {

        public static readonly StringSpan Empty = string.Empty;

        public readonly string? Source;
        public readonly int Offset;
        public readonly int Length;
        public StringSpan(string? source)
        {
            this.Source = source;
            this.Offset = 0;
            this.Length = source?.Length ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringSpan(string? buffer, int offset, int length)
        {
            if (buffer == null || (uint)offset > (uint)buffer.Length || (uint)length > (uint)(buffer.Length - offset))
            {
                throw new InvalidOperationException($"offset/length represents invalid string or string is null");
            }
            this.Source = buffer;
            this.Offset = offset;
            this.Length = length;
        }

        public string? Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (Source == null)
                    return Source;
                if (Offset == 0 && Length == Source.Length)
                    return Source;
                return Source.Substring(Offset, Length);
            }
        }

        public unsafe char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (Source == null || (uint)index >= (uint)Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                fixed (char* src = Source)
                {
                    char* charAt = src + Offset + index;
                    return *charAt;
                }
            }
        }

        public unsafe static string Concat(in StringSpan a, string value)
        {
            var alen = a.Length;
            var n = alen + value.Length;
            var s = new string('\0', n);
            fixed (char* dest = s)
            {
                fixed(char* aa = a.Source)
                {
                    char* astart = aa + a.Offset;
                    for (int i = 0; i < alen; i++)
                    {
                        dest[i] = astart[i];
                    }
                }
                fixed(char* aa = value)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        dest[alen + i] = aa[i];
                    }
                }
            }
            return s;
        }

        public static int Compare(in StringSpan a, in StringSpan b, StringComparison comparisonType)
        {
            int minLength = Math.Min(a.Length, b.Length);
            int diff = string.Compare(a.Source, a.Offset, b.Source, b.Offset, minLength, comparisonType);
            if (diff == 0)
            {
                diff = a.Length - b.Length;
            }

            return diff;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is StringSpan segment && Equals(in segment, StringComparison.Ordinal);
        }

        public ReadOnlySpan<Char> AsSpan()
        {
            return Source.AsSpan().Slice(Offset, Length);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        internal bool IsNullOrWhiteSpace()
        {
            return Source == null || string.IsNullOrWhiteSpace(Value);
        }

        public bool IsEmpty => Length == 0;

        public static implicit operator StringSpan(string source)
        {
            return new StringSpan(source);
        }

        public static bool operator ==(in StringSpan left, in StringSpan right)
        {
            return left.Equals(in right, StringComparison.Ordinal);
        }
        public static bool operator !=(in StringSpan left, in StringSpan right)
        {
            return !left.Equals(in right, StringComparison.Ordinal);
        }

        public static StringSpan operator +(in StringSpan left, in StringSpan right)
        {
            return new StringSpan(left.Value + right.Value);
        }

        public static StringSpan operator +(double left, in StringSpan right)
        {
            return new StringSpan(left.ToString() + right.Value);
        }


        public bool Equals(StringSpan other) => Equals(in other, StringComparison.Ordinal);

        public bool Equals(in StringSpan other, StringComparison comparisonType)
        {
            if (Length != other.Length)
            {
                return false;
            }

            return string.Compare(Source, Offset, other.Source, other.Offset, other.Length, comparisonType) == 0;
        }

        public static bool Equals(in StringSpan a, in StringSpan b, StringComparison comparisonType)
        {
            return a.Equals(in b, comparisonType);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(string other)
        {
            return Equals(other, StringComparison.Ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(string text, StringComparison comparisonType)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            int textLength = text.Length;
            if (Source == null || Length != textLength)
            {
                return false;
            }

            return string.Compare(Source, Offset, text, 0, textLength, comparisonType) == 0;
        }

        public int CompareTo(in StringSpan other)
        {
            if (Source == null)
            {
                if (other.Source == null)
                    return 0;
                return 1;
            }
            if (other.Source == null)
                return -1;
            var n = other.Length;
            return string.Compare(Source, Offset, other.Source, other.Offset, 
                Length > n 
                ? Length
                : n, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return Source.UnsafeGetHashCode(Offset, Length);
            // return Value?.GetHashCode() ?? 0;
        }

        public StringSpanReader Reader()
        {
            return new StringSpanReader(this);
        }
        public StringSpan Substring(int index)
        {
            return new StringSpan(Source, Offset + index, Length - index);
        }

        public CharEnumerator GetEnumerator()
        {
            return new CharEnumerator(this);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct CharEnumerator: IEnumerator<char>
        {
            private readonly StringSpan span;
            private int index;

            public CharEnumerator(in StringSpan span)
            {
                this.span = span;
                this.index = -1;
            }

            public unsafe bool MoveNext(out char ch)
            {
                this.index++;
                if (this.index >= span.Length)
                {
                    ch = '\0';
                    return false;
                }
                fixed (char* start = span.Source)
                {
                    char* ch1 = start + (span.Offset + index);
                    ch = *ch1;
                    return true;
                }

            }

            public char Current => UnsafeChar();

            private unsafe char UnsafeChar()
            {
                fixed(char* start = span.Source)
                {
                    char* ch = start + (span.Offset + index);
                    return *ch;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                // throw new NotImplementedException();
            }

            public bool MoveNext()
            {
                return ++this.index < span.Length;
            }

            public void Reset()
            {
                // throw new NotImplementedException();
            }
        }
    }
}

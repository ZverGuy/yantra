﻿using System;
using System.Collections.Generic;
using System.Text;
using WebAtoms.CoreJS.Extensions;
using WebAtoms.CoreJS.Utils;

namespace WebAtoms.CoreJS.Core.Runtime
{
    public class JSNumberStatic
    {
        [Static("isFinite")]
        public static JSValue IsFinite(JSValue t, params JSValue[] a)
        {
            if (a[0] is JSNumber n)
            {
                if (n.value != double.NaN && n.value > Double.NegativeInfinity && n.value < double.PositiveInfinity)
                    return JSBoolean.True;
            }
            return JSBoolean.False;
        }

        [Static("isInteger")]
        public static JSValue IsInteger(JSValue t, params JSValue[] a)
        {
            if (a[0] is JSNumber n)
            {
                var v = n.value;
                if (((int)v) == v)
                    return JSBoolean.True;
            }
            return JSBoolean.False;
        }

        [Static("isNaN")]
        public static JSValue IsNaN(JSValue t, params JSValue[] a)
        {
            if (a[0] is JSNumber n)
            {
                if (double.IsNaN(n.value))
                    return JSBoolean.True;
            }
            return JSBoolean.False;
        }

        [Static("isSafeInteger")]
        public static JSValue IsSafeInteger(JSValue t, params JSValue[] a)
        {
            if (a[0] is JSNumber n)
            {
                var v = n.value;
                if (v >= JSNumber.MinSafeInteger && v <= JSNumber.MaxSafeInteger)
                    return JSBoolean.True;
            }
            return JSBoolean.False;
        }

        [Static("parseFloat")]

        public static JSValue ParseFloat(JSValue t, params JSValue[] a)
        {
            var nan = JSNumber.NaN;
            if (a.Length > 0)
            {
                var p = a[0];
                if (p.IsNumber)
                    return p;
                if (p.IsNull || p.IsUndefined)
                    return nan;
                var text = p.JSTrim();
                if (text.Length > 0)
                {
                    int start = 0;
                    char ch;
                    bool hasDot = false;
                    bool hasE = false;
                    do
                    {
                        ch = text[start];
                        if (char.IsDigit(ch))
                        {
                            start++;
                            continue;
                        }
                        if (ch == '.')
                        {
                            if (!hasDot)
                            {
                                hasDot = true;
                                start++;
                                continue;
                            }
                            break;
                        }
                        if (ch == 'E' || ch == 'e')
                        {
                            if (!hasE)
                            {
                                hasE = true;
                                start++;
                                if (start < text.Length)
                                {
                                    var next = text[start];
                                    if (next == '+' || next == '-')
                                    {
                                        start++;
                                        continue;
                                    }
                                }
                                continue;
                            }
                            break;
                        }
                        break;
                    } while (start < text.Length);
                    if (text.Length > start)
                        text = text.Substring(0, start);
                    if (text.EndsWith("e+"))
                        text += "0";
                    if (text.EndsWith("e"))
                        text += "+0";
                    if (double.TryParse(text, out var d))
                    {
                        return new JSNumber(d);
                    }
                    return nan;
                }
            }
            return nan;
        }


        [Static("parseInt")]

        public static JSValue ParseInt(JSValue t, params JSValue[] a)
        {
            var nan = JSNumber.NaN;
            if (a.Length > 0)
            {
                var p = a[0];
                if (p.IsNumber)
                    return p;
                if (p.IsNull || p.IsUndefined)
                    return nan;
                var text = p.JSTrim();
                if (text.Length > 0)
                {
                    var radix = 10;
                    if (a.Length > 2)
                    {
                        var a1 = a[1];
                        if (a1.IsNull || a1.IsUndefined)
                        {
                            radix = 10;
                        }
                        else
                        {
                            var n = a1.DoubleValue;
                            if (!double.IsNaN(n))
                            {
                                radix = (int)n;
                                if (radix < 0 || radix == 1 || radix > 36)
                                    return nan;
                            }
                        }
                    }
                    var d = NumberParser.ParseInt(text.Trim(), radix, false);
                    return new JSNumber(d);
                }
            }
            return nan;
        }
    }
}
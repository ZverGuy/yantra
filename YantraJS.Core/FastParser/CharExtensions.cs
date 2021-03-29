﻿using System;
namespace YantraJS.Core.FastParser
{
    internal static class CharExtensions
    {

        internal static bool IsStringStart(this char ch)
        {
            return ch == '\'' || ch == '"';
        }

        internal static bool IsDigitStart(this char ch)
        {
            return char.IsDigit(ch);
        }

        internal static string FromCodePoint(this int cp)
        {
            return Char.ConvertFromUtf32(cp);
        }

        internal static int HexValue(this char ch)
        {
            if (ch >= 'A')
            {
                if (ch >= 'a')
                {
                    if (ch <= 'h')
                    {
                        return ch - 'a' + 10;
                    }
                }
                else if (ch <= 'H')
                {
                    return ch - 'A' + 10;
                }
            }
            else if (ch <= '9')
            {
                return ch - '0';
            }

            return 0;
        }

        internal static bool IsDigitPart(this char ch, bool hex, bool binary, bool readDecimal)
        {
            switch (ch)
            {
                case '_':
                case '0':
                case '1':
                    return true;
                case '.':
                    return readDecimal;
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    if (binary)
                    {
                        return false;
                    }
                    return true;
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                    return hex;
            }
            return false;

        }

        internal static bool IsIdentifierStart(this char ch)
        {
            switch (ch)
            {
                case '_':
                case '$':
                case '@':
                    return true;
            }
            return char.IsLetter(ch);
        }

        internal static bool IsIdentifierPart(this char ch)
        {
            switch (ch)
            {
                case '_':
                case '$':
                case '@':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return true;
            }
            return char.IsLetter(ch);
        }
    }
}

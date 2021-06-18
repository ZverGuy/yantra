﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace YantraJS.Core.FastParser
{
    /// <summary>
    /// This class will provide stream of tokens, we are using this instead of
    /// scanner directly as we can move scanning process in different thread
    /// in future.
    /// </summary>
    
    public class FastTokenStream
    {
        private readonly FastScanner scanner;
        public readonly FastPool Pool;

        internal Exception Unexpected()
        {
            var c = Current;
            return new FastParseException(c, $"Unexpected token {c.Type}: {c.Span} at {c.Start}");
        }

        public override string ToString()
        {
            return $"{Current} {Next}";
        }

        public readonly FastKeywordMap Keywords;
        private readonly List<FastToken> tokens;
        private int index;

        public FastTokenStream(in StringSpan text, FastKeywordMap keywords = null)
        {
            this.Pool = new FastPool();
            tokens = new List<FastToken>(Math.Max(1, text.Length/4));
            index = 0;
            this.Keywords = keywords ?? FastKeywordMap.Instance;
            this.scanner = new FastScanner(Pool, text, Keywords);
        }

        internal bool LineTerminator()
        {
            var m = SkipNewLines();
            if (m.LinesSkipped)
                return true;
            m.Undo();
            return false;
        }

        public FastTokenStream(FastPool pool, in StringSpan text, FastKeywordMap keywords = null)
        {
            this.Pool = pool;
            tokens = new List<FastToken>(Math.Max(1, text.Length / 4));
            index = 0;
            this.Keywords = keywords ?? FastKeywordMap.Instance;
            this.scanner = new FastScanner(pool, text, Keywords);
        }

        private FastToken this[int index]
        {
            get
            {
                while (tokens.Count <= index)
                {
                    tokens.Add(scanner.Token);
                    scanner.ConsumeToken();
                }
                return tokens[index];
            }
        }

        public FastToken Current => this[index];

        public FastToken Next => this[index + 1];

        public FastToken Previous => this[index > 0 ? index - 1 : index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastToken Expect(TokenTypes type)
        {
            SkipNewLines();
            var c = this[index];
            if (c.Type != type)
                throw new FastParseException(c, $"Expecting {type} at {c.Start.Line}, {c.Start.Column}");
            Consume();
            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastToken Expect(FastKeywords type)
        {
            SkipNewLines();
            var c = this[index];
            if (c.Keyword != type)
                throw new FastParseException(c, $"Expecting keyword {type} at {c.Start.Line}, {c.Start.Column}");
            Consume();
            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastToken ExpectContextualKeyword(FastKeywords type)
        {
            SkipNewLines();
            var c = this[index];
            if (c.ContextualKeyword != type) 
                throw new FastParseException(c, $"Expecting keyword {type} at {c.Start.Line}, {c.Start.Column}");
            Consume();
            return c;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsumeContextualKeyword(FastKeywords keyword)
        {
            var m = SkipNewLines();
            var c = this[index];
            if (c.ContextualKeyword == keyword)
            {
                Consume();
                return true;
            }
            m.Undo();
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsume(FastKeywords keyword)
        {
            var m = SkipNewLines();
            var c = this[index];
            if (c.Keyword == keyword)
            {
                Consume();
                return true;
            }
            m.Undo();
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsume(TokenTypes type)
        {
            var m = SkipNewLines();
            var c = this[index];
            if (c.Type == type)
            {
                Consume();
                return true;
            }
            m.Undo();
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsumeAny(TokenTypes type1, TokenTypes type2)
        {
            var m = SkipNewLines();
            var c = this[index].Type;
            if (c == type1 ||  c == type2)
            {
                Consume();
                return true;
            }
            m.Undo();
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsumeWithLineTerminator(TokenTypes type)
        {
            var m = SkipNewLines();
            var c = this[index].Type;
            if (c == type)
            {
                Consume();
                return true;
            }
            if (m.LinesSkipped)
            {
                return true;
            }
            m.Undo();
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsumeWithLineTerminator(TokenTypes type1, TokenTypes type2)
        {
            var m = SkipNewLines();
            var c = this[index].Type;
            if (c == type1 || c == type2)
            {
                Consume();
                return true;
            }
            if (m.LinesSkipped)
            {
                return true;
            }
            m.Undo();
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsumeAny(TokenTypes type1, TokenTypes type2, TokenTypes type3)
        {
            var m = SkipNewLines();
            var c = this[index].Type;
            if (c == type1 || c == type2 || c == type3)
            {
                Consume();
                return true;
            }
            m.Undo();
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsumeAny(TokenTypes type1, TokenTypes type2, TokenTypes type3, TokenTypes type4)
        {
            var m = SkipNewLines();
            var ct = this[index].Type;
            if (ct == type1 || ct == type2 || ct == type3 || ct == type4)
            {
                Consume();
                return true;
            }
            m.Undo();
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsume(TokenTypes type, out FastToken token)
        {
            var m = SkipNewLines();
            var c = this[index];
            if (c.Type == type)
            {
                token = c;
                Consume();
                return true;
            }
            m.Undo();
            token = null;
            return false;
        }

        public readonly struct Marker
        {
            private readonly FastTokenStream stream;
            private readonly int index;
            public readonly bool LinesSkipped;

            public Marker(FastTokenStream stream, int index, bool linesSkipped)
            {
                this.stream = stream;
                this.index = index;
                this.LinesSkipped = linesSkipped;
            }

            public void Undo()
            {
                stream.index = index;
            }
        }

        public Marker SkipNewLines()
        {
            var index = this.index;
            var c = this[index].Type;
            bool skipped = false;
            while(c == TokenTypes.LineTerminator)
            {
                c = Consume().Type;
                skipped = true;
            }
            return new Marker(this, index, skipped);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndConsume(TokenTypes type1, TokenTypes type2, out FastToken token1, out FastToken token2)
        {
            var marker = SkipNewLines();
            var c = this[index];
            if (c.Type == type1)
            {
                token1 = c;
                SkipNewLines();
                c = this[index + 1];
                if(c.Type == type2)
                {
                    Consume();
                    Consume();
                    token2 = c;
                    return true;
                }
            }
            marker.Undo();
            token1 = null;
            token2 = null;
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastToken Consume()
        {
            index++;
            return this[index];
        }

        public CancellableDisposableAction UndoMark()
        {
            var i = index;
            return new CancellableDisposableAction(() => {
                index = i;
            });
        }

        public int Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return index;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Reset(int position)
        {
            index = position;
            return false;
        }
    }
}

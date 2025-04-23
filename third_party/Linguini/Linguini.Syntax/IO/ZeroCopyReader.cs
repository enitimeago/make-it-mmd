using System;
using Linguini.Shared.Util;

namespace Linguini.Syntax.IO
{
    /// <summary>
    /// Zero copy reader used to parse Fluent resources
    /// </summary>
    public class ZeroCopyReader
    {
        private readonly ReadOnlyMemory<char> _unconsumedData;
        private int _position;
        private int _row;

        public ZeroCopyReader(string text) : this(text.AsMemory())
        {
        }

        private ZeroCopyReader(ReadOnlyMemory<char> memory)
        {
            _unconsumedData = memory;
            _position = 0;
            _row = 1;
        }

        public int Position
        {
            get => _position;
            set => _position = value;
        }

        public int Row
        {
            get => _row;
            set => _row = value;
        }

        public bool IsNotEof => _position < _unconsumedData.Length;
        public bool IsEof => !IsNotEof;
        internal ReadOnlyMemory<char> GetData => _unconsumedData;

        public char? PeekChar(int offset = 0)
        {
            if (_unconsumedData.TryReadChar(_position + offset, out var c))
            {
                return c;
            }

            return null;
        }

        public int SkipBlankBlock()
        {
            var count = 0;
            while (true)
            {
                var start = _position;
                SkipBlankInline();
                if (!SkipEol())
                {
                    _position = start;
                    break;
                }

                count += 1;
            }

            return count;
        }

        public bool SeekEol()
        {
            var index = _unconsumedData.Span.Slice(_position).IndexOf('\n');
            if (index != -1)
            {
                _row += 1;
                _position += index + 1;
                return true;
            }

            _position = _unconsumedData.Length;
            return false;
        }

        public bool SkipEol()
        {
            if ('\n' == PeekChar())
            {
                _row += 1;
                _position += 1;
                return true;
            }

            if ('\r' == PeekChar()
                && '\n' == PeekChar(1))
            {
                _row += 1;
                _position += 2;
                return true;
            }

            return false;
        }

        public int SkipBlankInline()
        {
            var start = _position;
            while (_unconsumedData.TryReadChar(_position, out var c) && ' ' == c)
            {
                _position += 1;
            }

            return _position - start;
        }

        public bool ReadCharIf(char c)
        {
            if (_unconsumedData.TryReadChar(_position, out var c1) && c == c1)
            {
                if (c == '\n')
                {
                    _row += 1;
                }

                _position += 1;
                return true;
            }

            return false;
        }

        public ReadOnlyMemory<char> GetCommentLine()
        {
            var startPosition = _position;

            while (!IsEol())
            {
                _position += 1;
            }

            return _unconsumedData.Slice(startPosition, _position - startPosition);
        }

        private bool IsEol()
        {
            var chr = PeekChar();

            if ('\n' == chr) return true;
            if ('\r' == chr
                && '\n' == PeekChar(1))
            {
                return true;
            }

            return IsEof;
        }

        public ReadOnlyMemory<char> ReadSlice(int start, int end)
        {
            return _unconsumedData.Slice(start, end - start);
        }

        public string ReadSliceToStr(int start, int end)
        {
            return _unconsumedData.Slice(start, end - start).Span.ToString();
        }

        public void SkipToNextEntry()
        {
            while (_unconsumedData.TryReadChar(_position, out var c))
            {
                var newline = _position == 0
                              || '\n' == PeekChar(-1);

                if (newline)
                {
                    _row += 1;
                    if (c.IsAsciiAlphabetic() || c.IsOneOf('#', '-'))
                        break;
                }

                _position += 1;
            }
        }

        public int IndexOfAnyChar(ReadOnlySpan<char> values)
        {
            return _unconsumedData.Span.Slice(_position).IndexOfAny(values);
        }

        public bool TryPeekChar(out char c)
        {
            return _unconsumedData.TryReadChar(_position, out c);
        }

        public bool TryPeekCharAt(int pos, out char c)
        {
            return _unconsumedData.TryReadChar(pos, out c);
        }

        public char CurrentChar()
        {
            return _unconsumedData.Span[_position];
        }

        public void SkipBlank()
        {
            while (TryPeekChar(out var c))
            {
                if (' ' == c)
                {
                    _position += 1;
                }
                else if ('\n' == c)
                {
                    _position += 1;
                    _row += 1;
                }
                else if ('\r' == c && '\n' == PeekChar(1))
                {
                    _position += 2;
                    _row += 1;
                }
                else
                {
                    break;
                }
            }
        }

        public void DecrementRow(int offset)
        {
            var span = _unconsumedData.Slice(_position - offset, offset);
            foreach (var chr in span.ToArray())
            {
                if (chr == '\n')
                    _row -= 1;
            }
        }
    }
}

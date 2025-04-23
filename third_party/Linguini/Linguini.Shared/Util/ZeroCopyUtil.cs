using System;

namespace Linguini.Shared.Util
{
    /// <summary>
    /// Utils used for Zero copy parsing.
    /// </summary>
    public static class ZeroCopyUtil
    {
        public static bool TryReadChar(this ReadOnlyMemory<char> memory, int pos, out char c)
        {
            if (pos >= memory.Length)
            {
                c = default;
                return false;
            }

            c = memory.Span[pos];
            return true;
        }

        public static bool IsAsciiAlphabetic(this char c)
        {
            return IsInside(c, 'a', 'z')
                   || IsInside(c, 'A', 'Z');
        }

        public static bool IsAsciiHexdigit(this char c)
        {
            return IsInside(c, '0', '9')
                   || IsInside(c, 'a', 'f')
                   || IsInside(c, 'A', 'F');
        }

        public static bool IsAsciiUppercase(this char c)
        {
            return IsInside(c, 'A', 'Z');
        }

        public static bool IsAsciiDigit(this char c)
        {
            return IsInside(c, '0', '9');
        }

        public static bool IsNumberStart(this char c)
        {
            return IsInside(c, '0', '9') || c == '-';
        }

        public static bool IsCallee(this ReadOnlySpan<char> charSpan)
        {
            bool isCallee = true;
            foreach (var c in charSpan)
            {
                if (!(c.IsAsciiUppercase() || c.IsAsciiDigit() || c.IsOneOf('_', '-')))
                {
                    isCallee = false;
                    break;
                }
            }

            return isCallee;
        }

        public static bool IsIdentifier(this char c)
        {
            return IsInside(c, 'a', 'z')
                   || IsInside(c, 'A', 'Z')
                   || IsInside(c, '0', '9')
                   || c == '-' || c == '_';
        }

        public static bool IsOneOf(this char c, char c1, char c2)
        {
            return c == c1 || c == c2;
        }

        public static bool IsOneOf(this char c, char c1, char c2, char c3)
        {
            return c == c1 || c == c2 || c == c3;
        }

        public static bool IsOneOf(this char c, char c1, char c2, char c3, char c4)
        {
            return c == c1 || c == c2 || c == c3 || c == c4;
        }

#if NETSTANDARD2_1 || UNITY_2021_3_OR_NEWER
        // Polyfill for netstandard 2.1 until dotnet backports MemoryExtension
        public static ReadOnlyMemory<char> TrimEndPolyFill(this ReadOnlyMemory<char> memory)
            => memory.Slice(0, FindLastWhitespace(memory.Span));

        private static int FindLastWhitespace(ReadOnlySpan<char> span)
        {
            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                {
                    break;
                }
            }

            return end + 1;
        }
#endif

        private static bool IsInside(char c, char min, char max) => (uint)(c - min) <= (uint)(max - min);
    }
}

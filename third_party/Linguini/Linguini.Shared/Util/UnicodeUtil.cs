using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Linguini.Shared.Util
{
    public static class UnicodeUtil
    {
        const string UnknownChar = "�";

        /// <summary>
        /// Method to unescape a given memory from string literal
        /// and write its converted content to a given writer.
        /// </summary>
        /// <param name="value">Read only memory containing escaped strings</param>
        /// <param name="writer">Writer to which we write the results of escaping</param>
        public static void WriteUnescapedUnicode(ReadOnlyMemory<char> value, TextWriter writer)
        {
            var bytes = Encoding.UTF8.GetBytes(value.ToArray());
            var start = 0;
            var ptr = 0;
            while (ptr < bytes.Length)
            {
                // No `\` means safe to skip
                if (!bytes[ptr].Equals((byte)'\\'))
                {
                    ptr += 1;
                    continue;
                }

                if (start != ptr)
                {
                    writer.Write(Encoding.UTF8.GetChars(bytes[start..ptr]));
                }
                // With this we skip double `\\`
                ptr += 1;

                var newChar = UnknownChar;

                if (bytes[ptr].Equals((byte)'\\'))
                {
                    newChar = "\\";
                }
                else if (bytes[ptr].Equals((byte)'"'))
                {
                    newChar = "\"";
                }
                else if (bytes[ptr].Equals((byte)'u')
                         || bytes[ptr].Equals((byte)'U'))
                {
                    var seqStart = ptr + 1;
                    var length = bytes[ptr] == (byte)'u' ? 4 : 6;
                    ptr += length;
                    newChar = EncodeUnicode(bytes, seqStart, seqStart + length);
                }

                ptr += 1;
                writer.Write(newChar);
                start = ptr;
            }

            if (start != ptr)
            {
                writer.Write(Encoding.UTF8.GetChars(bytes[start..ptr]));
            }
        }

        private static string EncodeUnicode(byte[] bytes, int start, int end)
        {
            // Start is inclusive end is exclusive
            // for bytes = {b, c, a} and start = 1 end = 2
            // we get {c, a}
            // if out of are out of range just return Replacement character
            if (start >= bytes.Length || end > bytes.Length)
                return UnknownChar;

            // this slices `004F` out of `\u004F`
            var codePointStr = Encoding.UTF8.GetString(bytes[start..end]);
            
            // Convert a value (e.g. `004F`) from hexadecimal to int to get approximate codepoint
            // convert codepoint to string (because it can be more than one UTF16 char)
            return !int.TryParse(codePointStr, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo,
                out var codePoint)
                ? UnknownChar
                : char.ConvertFromUtf32(codePoint);
        }
    }
}

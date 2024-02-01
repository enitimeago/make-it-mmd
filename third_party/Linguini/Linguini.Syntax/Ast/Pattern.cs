// Linguini
//
// MIT License
//
// Copyright 2021 Daniel Fath
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast
{
    public enum TextElementPosition : byte
    {
        InitialLineStart,
        LineStart,
        Continuation,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TextElementTermination : byte
    {
        LF,
        CRLF,
        PlaceableStart,
        EndOfFile
    }

    public enum TextElementType : byte
    {
        Blank,
        NonBlank,
    }

    public interface IPatternElementPlaceholder
    {
    }

    public interface IPatternElement
    {
        public static PatternComparer PatternComparer = new();
    }

    public class PatternComparer : IEqualityComparer<IPatternElement>
    {
        public bool Equals(IPatternElement? left, IPatternElement? right)
        {
            return (left, right) switch
            {
                (TextLiteral l, TextLiteral r) => l.Equals(r),
                (Placeable l, Placeable r) => l.Equals(r),
                _ => false,
            };
        }

        public int GetHashCode(IPatternElement obj)
        {
            switch (obj)
            {
                case TextLiteral textLiteral:
                    return textLiteral.GetHashCode();
                case Placeable placeable:
                    return placeable.GetHashCode();
                default:
                    throw new ArgumentException("Unexpected type", nameof(obj));
            }
        }
    }

    public class TextElementPlaceholder : IPatternElementPlaceholder
    {
        /// <summary>
        /// Start of text slice
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// End of text slice
        /// </summary>
        public int End { get; }
        /// <summary>
        /// Indent of Text element
        /// </summary>
        public int Indent { get; }
        
        /// <summary>
        /// Text element position used in pattern processing
        /// </summary>
        public TextElementPosition Role { get; }
        
        /// <summary>
        /// Boolean flag to add an EOL to text slice. It's used on Windows to make sure newlines are processed correctly
        /// </summary>
        public bool MissingEol { get; } 

        public TextElementPlaceholder(int start, int end, int indent, TextElementPosition role, bool missingEol)
        {
            Start = start;
            End = end;
            Indent = indent;
            Role = role;
            MissingEol = missingEol;
        }
    }
}

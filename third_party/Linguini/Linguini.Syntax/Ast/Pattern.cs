using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Linguini.Syntax.Ast
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

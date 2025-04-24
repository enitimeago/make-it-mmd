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
using System.Text;

using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Parser.Error
{
    public class ParseError
    {
        public ErrorType Kind { get; }
        public string Message { get; }
        public int Row { get; }
        public Range Position { get; }
        public Range? Slice { get; set; }

        private ParseError(ErrorType kind, string message, Range position, int row)
        {
            Kind = kind;
            Message = message;
            Position = position;
            Slice = null;
            Row = row;
        }

        public static ParseError ExpectedToken(char expected, char? actual, int pos, int row)
        {
            var sb = new StringBuilder()
                .AppendFormat("Expected a token starting with  \"{0}\" found \"{1}\" instead", expected,
                    actual.ToString());

            return new(
                ErrorType.ExpectedToken,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError ExpectedToken(char exp1, char exp2, char? actual, int pos, int row)
        {
            var sb = new StringBuilder()
                .AppendFormat("Expected  \"{0}\" or \"{1}\" found \"{2}\" instead",
                    exp1, exp2, actual.ToString());

            return new(
                ErrorType.ExpectedToken,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError ExpectedCharRange(string range, int pos, int row)
        {
            var sb = new StringBuilder().AppendFormat("Expected one of \"{0}\"", range);

            return new(
                ErrorType.ExpectedCharRange,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError ExpectedMessageField(ReadOnlyMemory<char> entryId, int start, int end, int row)
        {
            var sb = new StringBuilder()
                .Append(@"Expected a message field for """).Append(entryId).Append('"');

            return new(
                ErrorType.ExpectedMessageField,
                sb.ToString(),
                new Range(start, end),
                row
            );
        }

        public static ParseError MissingValue(int pos, int row)
        {
            return new(
                ErrorType.MissingValue,
                "Expected a value",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError UnbalancedClosingBrace(int pos, int row)
        {
            return new(
                ErrorType.UnbalancedClosingBrace,
                "Unbalanced closing brace",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError TermAttributeAsPlaceable(int pos, int row)
        {
            return new(
                ErrorType.TermAttributeAsPlaceable,
                "Term attributes can't be used as a selector",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError ExpectedTermField(Identifier id, int start, int end, int row)
        {
            var sb = new StringBuilder()
                .Append("Expected a term field for \"").Append(id.Name).Append("\"");

            return new(
                kind: ErrorType.ExpectedTermField,
                sb.ToString(),
                new Range(start, end),
                row
            );
        }

        public static ParseError MessageReferenceAsSelector(int pos, int row)
        {
            return new(
                ErrorType.MessageReferenceAsSelector,
                "Message references can't be used as a selector",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError MessageAttributeAsSelector(int pos, int row)
        {
            return new(
                ErrorType.MessageAttributeAsSelector,
                "Message attributes can't be used as a selector",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError TermReferenceAsSelector(int pos, int row)
        {
            return new(
                ErrorType.TermReferenceAsSelector,
                "Term references can't be used as a selector",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError ExpectedSimpleExpressionAsSelector(int pos, int row)
        {
            return new(
                ErrorType.ExpectedSimpleExpressionAsSelector,
                "Expected a simple expression as selector",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError UnterminatedStringLiteral(int pos, int row)
        {
            return new(
                ErrorType.UnterminatedStringLiteral,
                "Unterminated string literal",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError UnknownEscapeSequence(char seq, int pos, int row)
        {
            var sb = new StringBuilder()
                .AppendFormat("Unknown escape sequence \"{0}\"", seq);

            return new(
                ErrorType.UnknownEscapeSequence,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError ForbiddenCallee(int pos, int row)
        {
            return new(
                ErrorType.ForbiddenCallee,
                "Callee is not allowed here",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError ExpectedLiteral(int pos, int row)
        {
            return new(
                ErrorType.ExpectedLiteral,
                "Expected a string or number literal",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError ExpectedInlineExpression(int pos, int row)
        {
            return new(
                ErrorType.ExpectedInlineExpression,
                "Expected an inline expression",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError DuplicatedNamedArgument(Identifier id, int pos, int row)
        {
            var sb = new StringBuilder()
                .Append("The \"").Append(id.Name).Append("\" argument appears twice");

            return new(
                ErrorType.DuplicatedNamedArgument,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError PositionalArgumentFollowsNamed(int pos, int row)
        {
            return new(
                ErrorType.PositionalArgumentFollowsNamed,
                "Positional arguments must come before named arguments",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError MultipleDefaultVariants(int pos, int row)
        {
            return new(
                ErrorType.MultipleDefaultVariants,
                "A select expression can only have one default variant",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError MissingDefaultVariant(int pos, int row)
        {
            return new(
                ErrorType.MultipleDefaultVariants,
                "The select expression must have a default variant",
                new Range(pos, pos + 1),
                row
            );
        }

        public static ParseError InvalidUnicodeEscapeSequence(string seq, int readerPosition, int row)
        {
            return new(
                ErrorType.InvalidUnicodeEscapeSequence,
                $"Invalid unicode escape sequence, \"{seq}\"",
                new Range(readerPosition, readerPosition + 1),
                row
            );
        }
    }
}
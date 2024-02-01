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
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Parser.Error;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Errors
{
    public abstract record FluentError
    {
        public abstract ErrorType ErrorKind();

        public virtual ErrorSpan? GetSpan()
        {
            return null;
        }
    }

    /// <summary>
    /// Record used to pretty print the error if possible
    /// </summary>
    /// <param name="Row">Row in which error occured</param>
    /// <param name="StartSpan">Start of error context</param>
    /// <param name="EndSpan">End of error context</param>
    /// <param name="StartMark">Start of error mark</param>
    /// <param name="EndMark">End of error mark</param>
    public record ErrorSpan(int Row, int StartSpan, int EndSpan, int StartMark, int EndMark)
    {
    }

    public record OverrideFluentError : FluentError
    {
        private readonly string _id;
        private readonly EntryKind _kind;

        public OverrideFluentError(string id, EntryKind kind)
        {
            _id = id;
            _kind = kind;
        }

        public override ErrorType ErrorKind()
        {
            return ErrorType.Overriding;
        }

        public override string ToString()
        {
            return $"For id:{_id} already exist entry of type: {_kind.ToString()}";
        }
    }

    public record ResolverFluentError : FluentError
    {
        private readonly string _description;
        private readonly ErrorType _kind;

        private ResolverFluentError(string desc, ErrorType kind)
        {
            _description = desc;
            _kind = kind;
        }

        public override ErrorType ErrorKind()
        {
            return _kind;
        }

        public override string ToString()
        {
            return _description;
        }

        public static ResolverFluentError NoValue(ReadOnlyMemory<char> idName)
        {
            return new($"No value: {idName.Span.ToString()}", ErrorType.NoValue);
        }

        public static ResolverFluentError NoValue(string pattern)
        {
            return new($"No value: {pattern}", ErrorType.NoValue);
        }

        public static ResolverFluentError UnknownVariable(VariableReference outType)
        {
            return new($"Unknown variable: ${outType.Id}", ErrorType.Reference);
        }

        public static ResolverFluentError TooManyPlaceables()
        {
            return new("Too many placeables", ErrorType.TooManyPlaceables);
        }

        public static ResolverFluentError Reference(IInlineExpression self)
        {
            // TODO only allow references here
            if (self is FunctionReference funcRef)
            {
                return new($"Unknown function: {funcRef.Id}()", ErrorType.Reference);
            }

            if (self is MessageReference msgRef)
            {
                if (msgRef.Attribute == null)
                {
                    return new($"Unknown message: {msgRef.Id}", ErrorType.Reference);
                }

                return new($"Unknown attribute: {msgRef.Id}.{msgRef.Attribute}", ErrorType.Reference);
            }

            if (self is TermReference termReference)
            {
                if (termReference.Attribute == null)
                {
                    return new($"Unknown term: -{termReference.Id}", ErrorType.Reference);
                }

                return new($"Unknown attribute: -{termReference.Id}.{termReference.Attribute}", ErrorType.Reference);
            }

            if (self is VariableReference varRef)
            {
                return new($"Unknown variable: ${varRef.Id}", ErrorType.Reference);
            }

            throw new ArgumentException($"Expected reference got ${self.GetType()}");
        }

        public static ResolverFluentError Cyclic(Pattern pattern)
        {
            return new($"Cyclic reference in {pattern.Stringify()} detected!", ErrorType.Cyclic);
        }

        public static ResolverFluentError MissingDefault()
        {
            return new("No default", ErrorType.MissingDefault);
        }
    }

    public record ParserFluentError : FluentError
    {
        private readonly ParseError _error;

        private ParserFluentError(ParseError error)
        {
            _error = error;
        }

        public static ParserFluentError ParseError(ParseError parseError)
        {
            return new(parseError);
        }

        public override ErrorType ErrorKind()
        {
            return ErrorType.Parser;
        }

        public override string ToString()
        {
            return _error.Message;
        }

        public override ErrorSpan? GetSpan()
        {
            if (_error.Slice == null)
                return null;

            return new(_error.Row, _error.Slice.Value.Start.Value, _error.Slice.Value.End.Value,
                _error.Position.Start.Value, _error.Position.End.Value);
        }
    }

    public enum EntryKind : byte
    {
        Message,
        Term,
        Func,
    }

    public static class EntryHelper
    {
        public static EntryKind ToKind(this IEntry self)
        {
            if (self is AstTerm)
            {
                return EntryKind.Term;
            }

            return self is AstMessage ? EntryKind.Message : EntryKind.Func;
        }
    }

    public enum ErrorType : byte
    {
        Parser,
        Reference,
        Cyclic,
        NoValue,
        TooManyPlaceables,
        Overriding,
        MissingDefault
    }
}

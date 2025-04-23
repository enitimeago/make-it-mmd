using System;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser.Error;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Linguini.Bundle.Errors
{
    /// <summary>
    /// Provides base record for Linguini's Fluent related errors.
    /// </summary>
    public abstract record FluentError
    {
        /// <summary>
        /// What type of error was encountered
        /// </summary>
        /// <returns>Enumeration describing error.</returns>
        public abstract ErrorType ErrorKind();

        /// <summary>
        /// Where in file was error encountered.
        /// </summary>
        /// <returns>Position of error in file</returns>
        public virtual ErrorSpan? GetSpan()
        {
            return null;
        }



        /// <summary>
        /// String representation of the error.
        /// </summary>
        /// <returns>Error represented as string</returns>
        public override string ToString() => $"Error (${ErrorKind()})";
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

    /// <summary>
    /// Represents an error that occurs when there is an attempt to override an existing entry in a FluentBundle.
    /// </summary>
    public record OverrideFluentError : FluentError
    {
        private readonly EntryKind _kind;
        private readonly string? _location;

        /// <summary>
        /// Id of term or message that was overriden.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Constructs an <see cref="OverrideFluentError"/>
        /// </summary>
        /// <param name="id">Duplicated identifier</param>
        /// <param name="kind">Enumeration showing what kind of identifier was duplicate <c>MESSAGE | TERM | FUNCTION</c></param>
        /// <param name="location">Location where the error originated. Due to backwards compatibility it defaults to null</param>
        public OverrideFluentError(string id, EntryKind kind, string? location = null)
        {
            Id = id;
            _kind = kind;
            _location = location;
        }

        /// <inheritdoc/>
        public override ErrorType ErrorKind()
        {
            return ErrorType.Overriding;
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return $"For id:{Id} already exist entry of type: {_kind.ToString()}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public record ResolverFluentError : FluentError
    {
        private readonly string _description;
        private readonly ErrorType _kind;
        private readonly string? _location;

        private ResolverFluentError(string desc, ErrorType kind, string? location)
        {
            _description = desc;
            _kind = kind;
            _location = location;
        }

        /// <inheritdoc/>
        public override ErrorType ErrorKind()
        {
            return _kind;
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return _description;
        }

        public static ResolverFluentError NoValue(ReadOnlyMemory<char> idName, string? location = null)
        {
            return new($"No value: {idName.Span.ToString()}", ErrorType.NoValue, location);
        }

        public static ResolverFluentError NoValue(string pattern, string? location = null)
        {
            return new($"No value: {pattern}", ErrorType.NoValue, location);
        }

        public static ResolverFluentError UnknownVariable(VariableReference outType, string? location = null)
        {
            return new($"Unknown variable: ${outType.Id}", ErrorType.Reference, location);
        }

        public static ResolverFluentError TooManyPlaceables(string? location = null)
        {
            return new("Too many placeables", ErrorType.TooManyPlaceables,  location);
        }

        public static ResolverFluentError Reference(IInlineExpression self, string? location = null)
        {
            return self switch
            {
                FunctionReference funcRef => new($"Unknown function: {funcRef.Id}()", ErrorType.Reference, location),
                MessageReference msgRef when msgRef.Attribute == null => new($"Unknown message: {msgRef.Id}",
                    ErrorType.Reference, location),
                MessageReference msgRef => new($"Unknown attribute: {msgRef.Id}.{msgRef.Attribute}",
                    ErrorType.Reference, location),
                TermReference termReference when termReference.Attribute == null => new(
                    $"Unknown term: -{termReference.Id}", ErrorType.Reference, location),
                TermReference termReference => new($"Unknown attribute: -{termReference.Id}.{termReference.Attribute}",
                    ErrorType.Reference, location),
                VariableReference varRef => new($"Unknown variable: ${varRef.Id}", ErrorType.Reference, location),
                _ => throw new ArgumentException($"Expected reference got ${self.GetType()}")
            };
        }

        public static ResolverFluentError Cyclic(Pattern pattern, string? location = null)
        {
            return new($"Cyclic reference in {pattern.Stringify()} detected!", ErrorType.Cyclic, location);
        }

        public static ResolverFluentError MissingDefault(string? location = null)
        {
            return new("No default", ErrorType.MissingDefault, location);
        }
    }

    public record ParserFluentError : FluentError
    {
        private readonly ParseError _error;
        private readonly string? _location;

        private ParserFluentError(ParseError error, string? location = null)
        {
            _error = error;
            _location = location;
        }

        public static ParserFluentError ParseError(ParseError parseError)
        {
            return new(parseError);
        }

        /// <inheritdoc/>
        public override ErrorType ErrorKind()
        {
            return ErrorType.Parser;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _error.Message;
        }

        /// <inheritdoc/>
        public override ErrorSpan? GetSpan()
        {
            if (_error.Slice == null)
                return null;

            return new(_error.Row, _error.Slice.Value.Start.Value, _error.Slice.Value.End.Value,
                _error.Position.Start.Value, _error.Position.End.Value);
        }
    }

    /// <summary>
    /// Enumeration showing which kind of Entry was duplicated.
    /// </summary>
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
            return self switch
            {
                AstTerm => EntryKind.Term,
                AstMessage => EntryKind.Message,
                _ =>  EntryKind.Func
            };
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
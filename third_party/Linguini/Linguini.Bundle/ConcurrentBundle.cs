using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

// ReSharper disable UnusedType.Global

namespace Linguini.Bundle
{
    /// <summary>
    /// Represents a thread-safe implementation of the FluentBundle class.
    ///
    /// `ConcurrentBundle` implements the <see cref="IReadBundle"/> interface.
    /// </summary>
    public sealed class ConcurrentBundle : FluentBundle, IEquatable<ConcurrentBundle>
    {
        internal ConcurrentDictionary<string, FluentFunction> Functions = new();
        internal ConcurrentDictionary<string, AstTerm> Terms = new();
        internal ConcurrentDictionary<string, AstMessage> Messages = new();

        /// <inheritdoc />
        protected override void AddMessageOverriding(AstMessage message)
        {
            Messages[message.GetId()] = message;
        }

        /// <inheritdoc />
        protected override void AddTermOverriding(AstTerm term)
        {
            Terms[term.GetId()] = term;
        }

        /// <inheritdoc />
        protected override bool TryAddTerm(AstTerm term, List<FluentError>? errors)
        {
            if (Terms.TryAdd(term.GetId(), term)) return true;
            errors ??= new List<FluentError>();
            errors.Add(new OverrideFluentError(term.GetId(), EntryKind.Term));
            return false;
        }

        /// <inheritdoc />
        protected override bool TryAddMessage(AstMessage message, List<FluentError>? errors)
        {
            if (Messages.TryAdd(message.GetId(), message)) return true;
            errors ??= new List<FluentError>();
            errors.Add(new OverrideFluentError(message.GetId(), EntryKind.Message));
            return false;
        }


        /// <inheritdoc />
        public override bool TryAddFunction(string funcName, ExternalFunction fluentFunction)
        {
            return Functions.TryAdd(funcName, fluentFunction);
        }

        /// <inheritdoc />
        public override void AddFunctionOverriding(string funcName, ExternalFunction fluentFunction)
        {
            Functions[funcName] = fluentFunction;
        }

        /// <inheritdoc />
        public override void AddFunctionUnchecked(string funcName, ExternalFunction fluentFunction)
        {
            if (Functions.TryAdd(funcName, fluentFunction)) return;
            throw new ArgumentException($"Function with name {funcName} already exist");
        }

        /// <inheritdoc />
        public override bool HasMessage(string identifier)
        {
            return Messages.ContainsKey(identifier);
        }

        /// <inheritdoc />
        public override bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message)
        {
            return Messages.TryGetValue(ident, out message);
        }

        /// <inheritdoc />
        public override bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term)
        {
            return Terms.TryGetValue(ident, out term);
        }


        /// <inheritdoc />
        public override bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function)
        {
            return Functions.TryGetValue(funcName, out function);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetMessageEnumerable()
        {
            return Messages.Keys;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetFuncEnumerable()
        {
            return Functions.Keys;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetTermEnumerable()
        {
            return Terms.Keys;
        }

        /// <inheritdoc/>
        internal override IDictionary<string, AstMessage> GetMessagesDictionary()
        {
            return Messages;
        }

        /// <inheritdoc/>
        internal override IDictionary<string, AstTerm> GetTermsDictionary()
        {
            return Terms;
        }

        /// <inheritdoc/>
        internal override IDictionary<string, FluentFunction> GetFunctionDictionary()
        {
            return Functions;
        }

        /// <inheritdoc/>
        public override FluentBundle DeepClone()
        {
            return new ConcurrentBundle
            {
                Functions = new ConcurrentDictionary<string, FluentFunction>(Functions),
                Terms = new ConcurrentDictionary<string, AstTerm>(Terms),
                Messages = new ConcurrentDictionary<string, AstMessage>(Messages),
                Culture = (CultureInfo)Culture.Clone(),
                Locales = new List<string>(Locales),
                UseIsolating = UseIsolating,
                TransformFunc = (Func<string, string>?)TransformFunc?.Clone(),
                FormatterFunc = (Func<IFluentType, string>?)FormatterFunc?.Clone(),
                MaxPlaceable = MaxPlaceable,
                EnableExtensions = EnableExtensions,
            };
        }
        
        /// <inheritdoc/>
        public bool Equals(ConcurrentBundle? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Functions.SequenceEqual(other.Functions) && Terms.SequenceEqual(other.Terms) &&
                   Messages.SequenceEqual(other.Messages);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is ConcurrentBundle other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Functions, Terms, Messages);
        }
    }
}
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
using System.Globalization;
using System.Linq;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Errors;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Types;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

// ReSharper disable UnusedType.Global

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle
{
    /// <summary>
    /// Represents a non-concurrent Fluent bundle.
    ///
    /// Modification aren't thread-safe.
    /// </summary>
    public sealed class NonConcurrentBundle : FluentBundle, IEquatable<NonConcurrentBundle>
    {
        internal Dictionary<string, FluentFunction> Functions = new();
        internal Dictionary<string, AstTerm> Terms = new();
        internal Dictionary<string, AstMessage> Messages = new();

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
            Functions.Add(funcName, fluentFunction);
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
            return Messages.Keys.ToArray();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetFuncEnumerable()
        {
            return Functions.Keys.ToArray();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetTermEnumerable()
        {
            return Terms.Keys.ToArray();
        }

        /// <inheritdoc />
        internal override IDictionary<string, AstMessage> GetMessagesDictionary()
        {
            return Messages;
        }

        /// <inheritdoc />
        internal override IDictionary<string, AstTerm> GetTermsDictionary()
        {
            return Terms;
        }

        /// <inheritdoc />
        internal override IDictionary<string, FluentFunction> GetFunctionDictionary()
        {
            return Functions;
        }


        /// <inheritdoc />
        public override FluentBundle DeepClone()
        {
            return new NonConcurrentBundle()
            {
                Functions = new Dictionary<string, FluentFunction>(Functions),
                Terms = new Dictionary<string, AstTerm>(Terms),
                Messages = new Dictionary<string, AstMessage>(Messages),
                Culture = (CultureInfo)Culture.Clone(),
                Locales = new List<string>(Locales),
                UseIsolating = UseIsolating,
                TransformFunc = (Func<string, string>?)TransformFunc?.Clone(),
                FormatterFunc = (Func<IFluentType, string>?)FormatterFunc?.Clone(),
                MaxPlaceable = MaxPlaceable,
                EnableExtensions = EnableExtensions,
            };
        }

        /// <inheritdoc />
        public bool Equals(NonConcurrentBundle? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Functions.SequenceEqual(other.Functions) && Terms.SequenceEqual(other.Terms) &&
                   Messages.SequenceEqual(other.Messages);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is NonConcurrentBundle other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Functions, Terms, Messages);
        }
    }
}
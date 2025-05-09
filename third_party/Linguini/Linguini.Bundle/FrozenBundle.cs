﻿// Linguini
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
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Errors;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Resolver;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Types;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#elif NET6_0_OR_GREATER
using System.Collections.Immutable;
#endif


namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle
{
    /// <summary>
    /// Represents a frozen bundle i.e. a bundle to which no items can be added or removed.
    ///
    /// It properly works on net8.0 utilizing its FrozenDictionary implementation.
    /// On other platforms it uses a very naive polyfill.
    /// Frozen bundle implements the <see cref="IReadBundle"/> interface.
    /// </summary>
    public class FrozenBundle : IReadBundle
    {
        /// <summary>
        /// <see cref="CultureInfo"/> of the bundle. Primary bundle locale
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// List of Locales. First element is primary bundle locale, others are fallback locales.
        /// </summary>
        public List<string> Locales { get; init; }

        /// <summary>
        /// When formatting patterns, FluentBundle inserts Unicode Directionality Isolation Marks to indicate that the direction of a placeable may differ from the surrounding message.
        /// This is important for cases such as when a right-to-left user name is presented in the left-to-right message.
        /// </summary>
        public bool UseIsolating { get; }

        /// <summary>
        /// Specifies a method that will be applied only on values extending <see cref="IFluentType"/>. Useful for defining a special formatter for <see cref="FluentNumber"/>.
        /// </summary>
        public Func<IFluentType, string>? FormatterFunc { get; }

        /// <summary>
        /// Limit of placeable <see cref="AstTerm"/> within one <see cref="Pattern"/>, when fully expanded (all nested elements count towards it). Useful for preventing billion laughs attack. Defaults to 100.
        /// </summary>
        public byte MaxPlaceable { get; }

        /// <summary>
        /// Whether experimental features are enabled.
        ///
        /// When `true` experimental features are enabled. Experimental features include stuff like:
        /// <list type="bullet">
        /// <item>dynamic reference</item>
        /// <item>dynamic reference attributes</item>
        /// <item>term reference as parameters</item>
        /// </list>
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public bool EnableExtensions { get; init; }

        /// <summary>
        /// Specifies a method that will be applied only on values extending <see cref="IFluentType"/>. Useful for defining a special formatter for <see cref="FluentNumber"/>.
        /// </summary>
        public Func<string, string>? TransformFunc { get; }

        internal readonly IDictionary<string, FluentFunction> Functions;
        internal readonly IDictionary<string, AstTerm> Terms;
        internal readonly IDictionary<string, AstMessage> Messages;
#if NET8_0_OR_GREATER
        internal FrozenBundle(FluentBundle bundle)
        {
            Culture = bundle.Culture;
            MaxPlaceable = bundle.MaxPlaceable;
            Locales = bundle.Locales;
            UseIsolating = bundle.UseIsolating;
            EnableExtensions = bundle.EnableExtensions;
            FormatterFunc = bundle.FormatterFunc;
            TransformFunc = bundle.TransformFunc;
            Messages = bundle.GetMessagesDictionary().ToFrozenDictionary();
            Terms = bundle.GetTermsDictionary().ToFrozenDictionary();
            Functions = bundle.GetFunctionDictionary().ToFrozenDictionary();
        }
#elif NET6_0_OR_GREATER
        internal FrozenBundle(FluentBundle bundle)
        {
            Culture = bundle.Culture;
            MaxPlaceable = bundle.MaxPlaceable;
            Locales = bundle.Locales;
            UseIsolating = bundle.UseIsolating;
            EnableExtensions = bundle.EnableExtensions;
            FormatterFunc = bundle.FormatterFunc;
            TransformFunc = bundle.TransformFunc;
            Messages = bundle.GetMessagesDictionary().ToImmutableDictionary();
            Terms = bundle.GetTermsDictionary().ToImmutableDictionary();
            Functions = bundle.GetFunctionDictionary().ToImmutableDictionary();
        }
#else
        internal FrozenBundle(FluentBundle bundle)
        {
            Culture = bundle.Culture;
            MaxPlaceable = bundle.MaxPlaceable;
            Locales = bundle.Locales;
            UseIsolating = bundle.UseIsolating;
            EnableExtensions = bundle.EnableExtensions;
            FormatterFunc = bundle.FormatterFunc;
            TransformFunc = bundle.TransformFunc;
            Messages = new Dictionary<string, AstMessage>(bundle.GetMessagesDictionary());
            Terms = new Dictionary<string, AstTerm>(bundle.GetTermsDictionary());
            Functions = new Dictionary<string, FluentFunction>(bundle.GetFunctionDictionary());
        }
#endif
        /// <inheritdoc/>
        public bool HasMessage(string identifier)
        {
            return Messages.ContainsKey(identifier);
        }

        /// <inheritdoc/>
        public string FormatPattern(Pattern pattern, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors)
        {
            var scope = new Scope(this, args);
            var value = pattern.Resolve(scope);
            errors = scope.Errors;
            return value.AsString();
        }

        /// <inheritdoc/>
        public bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message)
        {
            return Messages.TryGetValue(ident, out message);
        }

        /// <inheritdoc/>
        public bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term)
        {
            return Terms.TryGetValue(ident, out term);
        }

        /// <inheritdoc/>
        public bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? function)
        {
            return Functions.TryGetValue(id.ToString(), out function);
        }

        /// <inheritdoc/>
        public bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function)
        {
            return Functions.TryGetValue(funcName, out function);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetMessageEnumerable()
        {
            return Messages.Keys;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetFuncEnumerable()
        {
            return Functions.Keys;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetTermEnumerable()
        {
            return Terms.Keys;
        }
    }
}
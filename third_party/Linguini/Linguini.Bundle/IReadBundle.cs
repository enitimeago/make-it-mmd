using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

// ReSharper disable UnusedMemberInSuper.Global

namespace Linguini.Bundle
{
    /// <summary>
    /// Represents an interface for reading language bundles. Read-only bundles only implement this interface.
    /// </summary>
    public interface IReadBundle
    {
        /// <summary>
        /// Determines if the provided identifier has a message associated with it. </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns>True if the identifier has a message; otherwise, false.</returns>
        /// 
        bool HasMessage(string identifier);

        /// <summary>
        /// Converts a <see cref="Pattern"/> to a string using given arguments.
        /// </summary>
        /// <param name="pattern">The pattern to format.</param>
        /// <param name="args">The dictionary of arguments to replace the variables.</param>
        /// <param name="errors">The list of FluentErrors, if any occurred during formatting.</param>
        /// <returns>The formatted string.</returns>
        string FormatPattern(Pattern pattern, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors)
        {
            errors = null;
            return FormatPatternErrRef(pattern, args, ref errors);
        }

        /// <summary>
        /// Converts a <see cref="Pattern"/> to a string using given arguments.
        /// </summary>
        /// <param name="pattern">The pattern to format.</param>
        /// <param name="args">The dictionary of arguments to replace the variables.</param>
        /// <param name="errors">The reference to a list of FluentErrors, will be full if any occurred during formatting.</param>
        /// <returns>The formatted string.</returns>
        string FormatPatternErrRef(Pattern pattern, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] ref IList<FluentError>? errors);

        /// <summary>
        /// Tries to get the AstMessage associated with the specified ident.
        /// </summary>
        /// <param name="ident">The identifier to look for.</param>
        /// <param name="message">When this method returns, contains the AstMessage associated with the specified ident, if found; otherwise, null.</param>
        /// <returns>True if an AstMessage was found for the specified ident; otherwise, false.</returns>
        bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message);

        /// <summary>
        /// Tries to get a term by its identifier.
        /// </summary>
        /// <param name="ident">The identifier of the AST term.</param>
        /// <param name="term">When this method returns, contains the AST term associated with the specified identifier, if the identifier is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the identifier is found and the corresponding AST term is retrieved; otherwise, false.</returns>
        bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term);

        /// <summary>
        /// Tries to get the FluentFunction associated with the specified Identifier.
        /// </summary>
        /// <param name="id">The Identifier used to identify the FluentFunction.</param>
        /// <param name="function">When the method returns, contains the FluentFunction associated with the Identifier, if the Identifier is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the FluentFunction associated with the Identifier is found; otherwise, false.</returns>
        bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? function);

        /// <summary>
        /// Tries to retrieve a FluentFunction object by the given function name.
        /// </summary>
        /// <param name="funcName">The name of the function to retrieve.</param>
        /// <param name="function">An output parameter that will hold the retrieved FluentFunction object, if found.</param>
        /// <returns>
        /// True if a FluentFunction object with the specified name was found and assigned to the function output parameter.
        /// False if a FluentFunction object with the specified name was not found.
        /// </returns>
        bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function);

        /// <summary>
        /// This method retrieves an enumerable collection of all message identifiers. </summary>
        /// <returns>
        /// An enumerable collection of message identifiers. </returns>
        IEnumerable<string> GetMessageEnumerable();

        /// <summary>
        /// Retrieves an enumerable collection of string function names.
        /// </summary>
        /// <returns>An enumerable collection of functions names.</returns>
        IEnumerable<string> GetFuncEnumerable();

        /// <summary>
        /// Retrieves an enumerable collection of terms.
        /// </summary>
        /// <returns>An enumerable collection of terms.</returns>
        IEnumerable<string> GetTermEnumerable();

        /// <summary>
        /// Tries to retrieve a message based on the provided ID and arguments.
        /// A convenience method for <see cref="TryGetMessage(string, string?, System.Collections.Generic.IDictionary{string,Linguini.Shared.Types.Bundle.IFluentType}?,out System.Collections.Generic.IList{Linguini.Bundle.Errors.FluentError}?,out string?)"/>
        /// </summary>
        /// <param name="id">The ID of the message to retrieve.</param>
        /// <param name="args">Optional. A dictionary of arguments to be inserted into the message.</param>
        /// <param name="errors">Optional. When the method returns false, a list of errors encountered during the retrieval process.</param>
        /// <param name="message">Optional. When the method returns true, the retrieved message. Null if no message is found.</param>
        /// <returns>
        /// True if the message was successfully retrieved. False otherwise.
        /// </returns>
        bool TryGetMessage(string id, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            errors = null;
            return TryGetMessageErrRef(id, null, args, ref errors, out message);
        }


        /// <summary>
        /// Determines whether the given identifier with attribute has a message.
        /// </summary>
        /// <param name="idWithAttr">The identifier with attribute.</param>
        /// <returns>True if the identifier with attribute has a message; otherwise, false.</returns>
        bool HasAttrMessage(string idWithAttr)
        {
            var attributes = idWithAttr.IndexOf('.');
            if (attributes < 0)
            {
                return HasMessage(idWithAttr);
            }

            var id = idWithAttr.AsSpan(0, attributes).ToString();
            var attr = idWithAttr.AsSpan(attributes + 1).ToString();
            if (TryGetAstMessage(id, out var astMessage))
            {
                return astMessage.GetAttribute(attr) != null;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the attribute message by processing the given message template with the provided arguments.
        /// </summary>
        /// <param name="msgWithAttr">The string consisting of `messageId.Attribute`.</param>
        /// <param name="args">The dictionary of arguments to be used for resolution in the message template. Can be null.</param>
        /// <returns>The processed message.</returns>
        /// <exception cref="LinguiniException">Thrown when there are errors encountered during attribute substitution.</exception>
        string? GetAttrMessage(string msgWithAttr, params (string, IFluentType)[] args)
        {
            var dictionary = new Dictionary<string, IFluentType>(args.Length);
            foreach (var (key, val) in args)
            {
                dictionary.Add(key, val);
            }

            TryGetAttrMessage(msgWithAttr, dictionary, out var errors, out var message);
            if (errors is { Count: > 0 })
            {
                throw new LinguiniException(errors);
            }

            return message;
        }

        /// <summary>
        /// Tries to retrieve an attribute message.
        /// </summary>
        /// <param name="msgWithAttr">The message with attribute.</param>
        /// <param name="args">The arguments passed with the message.</param>
        /// <param name="errors">The list of errors that occurred during the message retrieval process.</param>
        /// <param name="message">The retrieved message.</param>
        /// <returns>True if the attribute message is found; otherwise, false.</returns>
        bool TryGetAttrMessage(string msgWithAttr, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            errors = null;
            return TryGetAttrMessageErrRef(msgWithAttr, args, ref errors, out message);
        }

        /// <summary>
        /// Tries to retrieve an attribute message.
        /// </summary>
        /// <param name="msgWithAttr">The message with attribute.</param>
        /// <param name="args">The arguments passed with the message.</param>
        /// <param name="errors">The list of errors that occurred during the message retrieval process.</param>
        /// <param name="message">The retrieved message.</param>
        /// <returns>True if the attribute message is found; otherwise, false.</returns>
        bool TryGetAttrMessageErrRef(string msgWithAttr, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] ref IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            if (msgWithAttr.Contains("."))
            {
                var split = msgWithAttr.Split('.');
                return TryGetMessage(split[0], split[1], args, out errors, out message);
            }

            return TryGetMessage(msgWithAttr, null, args, out errors, out message);
        }

        /// <summary>
        /// Tries to get a message based on the provided parameters.
        /// </summary>
        /// <param name="id">The identifier of the message.</param>
        /// <param name="attribute">The attribute of the message.</param>
        /// <param name="args">The arguments to format the message with.</param>
        /// <param name="errors">The list of errors that occurred during the message retrieval process.</param>
        /// <param name="message">The retrieved message.</param>
        /// <returns>True if the message was successfully retrieved, otherwise false.</returns>
        public bool TryGetMessage(string id, string? attribute, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            errors = null;
            return TryGetMessageErrRef(id, attribute, args, ref errors, out message);
        }

        /// <summary>
        /// Tries to get a message based on the provided parameters.
        /// </summary>
        /// <param name="id">The identifier of the message.</param>
        /// <param name="attribute">The attribute of the message.</param>
        /// <param name="args">The arguments to format the message with.</param>
        /// <param name="errors">The reference to list of errors that occurred during the message retrieval process.</param>
        /// <param name="message">The retrieved message.</param>
        /// <returns>True if the message was successfully retrieved, otherwise false.</returns>
        public bool TryGetMessageErrRef(string id, string? attribute, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] ref IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            errors = new List<FluentError>();
            var msg = attribute == null
                ? id
                : $"{id}.{attribute}";

            if (!TryGetAstMessage(id, out var astMessage))
            {
                errors.Add(ResolverFluentError.NoValue($"{msg}"));
                message = null;
                return false;
            }

            var pattern = attribute != null
                ? astMessage.GetAttribute(attribute)?.Value
                : astMessage.Value;

            if (pattern == null)
            {
                errors.Add(ResolverFluentError.NoValue($"{msg}"));
                message = FluentNone.None.ToString();
                return false;
            }

            errors = null;
            message = FormatPattern(pattern, args, out errors);
            return true;
        }
    }

    /// <summary>
    /// Provides extension methods for working with bundles.
    /// </summary>
    public static class ReadBundleExtensions
    {
        /// <summary>
        /// Thaws a frozen bundle and returns a new instance of a concurrent bundle.
        /// </summary>
        /// <param name="frozenBundle">The frozen bundle to thaw.</param>
        /// <returns>A new instance of a concurrent bundle.</returns>
        public static ConcurrentBundle Thaw(this FrozenBundle frozenBundle)
        {
            return new ConcurrentBundle
            {
                Messages = new ConcurrentDictionary<string, AstMessage>(frozenBundle.Messages),
                Functions = new ConcurrentDictionary<string, FluentFunction>(frozenBundle.Functions),
                Terms = new ConcurrentDictionary<string, AstTerm>(frozenBundle.Terms),
                FormatterFunc = frozenBundle.FormatterFunc,
                Locales = frozenBundle.Locales,
                UseIsolating = frozenBundle.UseIsolating,
                MaxPlaceable = frozenBundle.MaxPlaceable,
                EnableExtensions = frozenBundle.EnableExtensions,
                TransformFunc = frozenBundle.TransformFunc,
                Culture = frozenBundle.Culture
            };
        }

        /// <summary>
        /// Convenience method for <see cref="IReadBundle.HasAttrMessage"/>
        /// </summary>
        /// <param name="bundle">The bundle to retrieve the message from.</param>
        /// <param name="idWithAttr">The identifier with attribute.</param>
        /// <returns>True if the identifier with attribute has a message; otherwise, false.</returns>
        public static bool HasAttrMessage(this IReadBundle bundle, string idWithAttr)
        {
            return bundle.HasAttrMessage(idWithAttr);
        }

        /// <summary>
        /// Convenience method for <see cref="IReadBundle.GetAttrMessage"/>
        /// </summary>
        /// <param name="bundle">The bundle to retrieve the message from.</param>
        /// <param name="msgWithAttr">The message with attribute to retrieve.</param>
        /// <param name="args">Optional arguments to format the message.</param>
        /// <returns>The attribute message from the read bundle.</returns>
        public static string? GetAttrMessage(this IReadBundle bundle, string msgWithAttr,
            params (string, IFluentType)[] args)
        {
            return bundle.GetAttrMessage(msgWithAttr, args);
        }


        /// <summary>
        /// Retrieves a localized message from the given bundle.
        /// </summary>
        /// <param name="bundle">The bundle to retrieve the message from.</param>
        /// <param name="id">The ID of the message to retrieve.</param>
        /// <param name="attribute">The optional attribute of the message. Defaults to null.</param>
        /// <param name="args">The optional dictionary of arguments to be used in the message resolution. Defaults to null.</param>
        /// <returns>The localized message if found, otherwise null.</returns>
        /// <exception cref="LinguiniException">Thrown when there are errors retrieving the message.</exception>
        public static string? GetMessage(this IReadBundle bundle, string id, string? attribute = null,
            IDictionary<string, IFluentType>? args = null)
        {
            bundle.TryGetMessage(id, attribute, args, out var errors, out var message);
            if (errors is { Count: > 0 })
            {
                throw new LinguiniException(errors);
            }

            return message;
        }

        /// <summary>
        /// Convenience method for <see cref="IReadBundle.TryGetAttrMessage"/>
        /// </summary>
        /// <param name="bundle">The bundle to retrieve the message from.</param>
        /// <param name="msgWithAttr">The message with attribute</param>
        /// <param name="args">Optional arguments to be passed to the attribute message.</param>
        /// <param name="errors">When this method returns false, contains a list of errors that occurred while trying to retrieve the message; otherwise, null.</param>
        /// <param name="message">When this method returns true, contains the retrieved message; otherwise, null.</param>
        /// <returns><c>true</c> if the attribute message was successfully retrieved; otherwise, <c>false</c>.</returns>
        public static bool TryGetAttrMessage(this IReadBundle bundle, string msgWithAttr,
            IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            return bundle.TryGetAttrMessage(msgWithAttr, args, out errors, out message);
        }

        /// <summary>
        /// Convenience method for <see cref="IReadBundle.TryGetMessage(string,string?,System.Collections.Generic.IDictionary{string,Linguini.Shared.Types.Bundle.IFluentType}?,out System.Collections.Generic.IList{Linguini.Bundle.Errors.FluentError}?,out string?)"/>
        /// </summary>
        /// <param name="bundle">The bundle to retrieve the message from.</param>
        /// <param name="id">The identifier of the message.</param>
        /// <param name="attribute">Optional attribute of the message.</param>
        /// <param name="args">Optional arguments to be passed to the attribute message.</param>
        /// <param name="errors">When this method returns false, contains a list of errors that occurred while trying to retrieve the message; otherwise, null.</param>
        /// <param name="message">When this method returns true, contains the retrieved message; otherwise, null.</param>
        /// <returns>True if the message was successfully retrieved, otherwise false.</returns>
        public static bool TryGetMessage(this IReadBundle bundle, string id, string? attribute,
            IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            return bundle.TryGetMessage(id, attribute, args, out errors, out message);
        }

        /// <summary>
        /// Tries to get a message from the specified bundle.
        /// </summary>
        /// <param name="bundle">The bundle from which to retrieve the message.</param>
        /// <param name="id">The identifier of the message to retrieve.</param>
        /// <param name="args">Optional arguments used to format the message (optional).</param>
        /// <param name="errors">When this method returns false, contains a list of errors that occurred while trying to retrieve the message; otherwise, null.</param>
        /// <param name="message">When this method returns true, contains the retrieved message; otherwise, null.</param>
        /// <returns>True if the message was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetMessage(this IReadBundle bundle, string id,
            IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            return bundle.TryGetMessage(id, null, args, out errors, out message);
        }
    }
}
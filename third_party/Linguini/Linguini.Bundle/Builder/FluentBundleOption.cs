using System;
using System.Collections.Generic;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Builder
{
    /// <summary>
    /// Represents the options for configuring a FluentBundle.
    /// </summary>
    public class FluentBundleOption
    {
        /// <summary>
        /// Specifies whether the FluentBundle is thread-safe or not.
        ///
        /// When true, it will use <see cref="ConcurrentBundle"/>, otherwise it will use <see cref="FluentBundle"/>
        /// </summary>
        public bool UseConcurrent { get; init; }
        
        /// <summary>
        /// Enables Extensions to standard Fluent syntax.
        ///
        /// When true it enables following features:
        /// <list type="bullet">
        /// <item><b>Dynamic Reference</b> - ability to reference terms/message using <c>$$term_ref</c>.</item>
        /// <item><b>Dynamic Reference attribute</b> - ability to reference terms/message using <c>$$term_ref.Attribute</c>.</item>
        /// <item><b>Term passing</b> - <c>-term.attribute(arg0: $passed_term)</c>.</item>
        /// </list>
        /// </summary>
        /// <value>Enables the non-standard Fluent syntax. Defaults to <c>false</c>.</value>
        public bool EnableExtensions { get; init; }
        
        /// <summary>
        /// Use Unicode isolation.
        /// 
        /// Most of the time the Unicode BiDi Algorithm handles bidirectional text very well. In some cases it needs
        /// some help, so this adds isolating characters to ensure correct behavior.
        /// For more detalis see: https://github.com/projectfluent/fluent.js/wiki/Unicode-Isolation
        /// </summary>
        /// <value>
        /// Enables bidirectional text isolation. Defaults to <c>true</c>.
        /// </value>
        public bool UseIsolating { get; init; } = true;
        
        /// <summary>
        /// Maximal number of Placeable(s).
        ///
        /// It prevents the deep nesting of terms, which may lead to https://en.wikipedia.org/wiki/Billion_laughs_attack .
        /// </summary>
        /// <value>
        /// Number of nested placeable values, defaults to one hundred.
        /// </value>
        public byte MaxPlaceable { get; init; } = 100;


        /// <summary>
        /// Represents the list of locales for the FluentBundle.
        /// </summary>
        /// <value>
        /// The locales define the languages and regional variations that the FluentBundle supports.
        /// </value>
        public List<string> Locales { get; init; } = new List<string>();

        /// <summary>
        /// Specifies the external functions that can be used in a FluentBundle.
        /// </summary>
        /// <value>
        /// The dictionary containing the external functions. The key represents
        /// the function name, and the value is a delegate of type
        /// <see cref="ExternalFunction"/>.
        /// </value>
        public IDictionary<string, ExternalFunction> Functions { get; init; } =
            new Dictionary<string, ExternalFunction>();

        /// <summary>
        /// Gets or sets the formatter function that is used to format Fluent type values into strings.
        /// </summary>
        /// <remarks>
        /// The formatter function takes an instance of <see cref="IFluentType"/> and returns the string representation of the value.
        /// <para>
        /// If not set, the default formatting behavior will be used.
        /// </para>
        /// </remarks>
        /// <value>
        /// The formatter function.
        /// </value>
        public Func<IFluentType, string>? FormatterFunc { get; init; }

        /// <summary>
        /// Represents a function that transforms a string value before it is used in Fluent message formatting.
        /// </summary>
        /// <value>The transformed string value.</value>
        public Func<string, string>? TransformFunc { get; init; }
    }
}

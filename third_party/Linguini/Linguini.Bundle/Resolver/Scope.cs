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
using System.IO;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Errors;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Resolver
{
    public class Scope : IScope
    {
        public readonly IReadBundle Bundle;
        private readonly CultureInfo _culture;
        private readonly int _maxPlaceable;
        private readonly Dictionary<string, IFluentType>? _args;
        private Dictionary<string, IFluentType>? _localNameArgs;
        private List<IFluentType>? _localPosArgs;
        private readonly List<Pattern> _travelled;
        private readonly List<FluentError> _errors;


        public Scope(FluentBundle fluentBundle, IDictionary<string, IFluentType>? args)
        {
            Placeable = 0;
            Bundle = fluentBundle;
            _maxPlaceable = fluentBundle.MaxPlaceable;
            _culture = fluentBundle.Culture;
            TransformFunc = fluentBundle.TransformFunc;
            FormatterFunc = fluentBundle.FormatterFunc;
            UseIsolating = fluentBundle.UseIsolating;
            FormatterFunc = fluentBundle.FormatterFunc;
            Dirty = false;

            _errors = new List<FluentError>();
            _travelled = new List<Pattern>();
            _args = args != null ? new Dictionary<string, IFluentType>(args) : null;

            _localNameArgs = null;
            _localPosArgs = null;
            _errors = new List<FluentError>();
        }


        public Scope(FrozenBundle frozenBundle, IDictionary<string, IFluentType>? args)
        {
            Placeable = 0;
            Bundle = frozenBundle;
            _maxPlaceable = frozenBundle.MaxPlaceable;
            _culture = frozenBundle.Culture;
            TransformFunc = frozenBundle.TransformFunc;
            FormatterFunc = frozenBundle.FormatterFunc;
            UseIsolating = frozenBundle.UseIsolating;
            Dirty = false;

            _errors = new List<FluentError>();
            _travelled = new List<Pattern>();
            _args = args != null ? new Dictionary<string, IFluentType>(args) : null;

            _localNameArgs = null;
            _localPosArgs = null;
            _errors = new List<FluentError>();
        }


        public bool Dirty { get; set; }
        private short Placeable { get; set; }

        public IList<FluentError> Errors => _errors;

        public IReadOnlyDictionary<string, IFluentType>? LocalNameArgs => _localNameArgs;
        public IReadOnlyList<IFluentType>? LocalPosArgs => _localPosArgs;

        public IReadOnlyDictionary<string, IFluentType>? Args => _args;
        public Func<IFluentType, string>? FormatterFunc { get; }
        public Func<string, string>? TransformFunc { get; }


        public bool UseIsolating { get; init; }


        public bool IncrPlaceable()
        {
            return ++Placeable <= _maxPlaceable;
        }

        public void AddError(ResolverFluentError resolverFluentError)
        {
            _errors.Add(resolverFluentError);
        }

        public void MaybeTrack(TextWriter writer, Pattern pattern, IExpression expr, int pos)
        {
            if (_travelled.Count == 0)
            {
                _travelled.Add(pattern);
            }

            expr.TryWrite(writer, this, pos);

            if (Dirty)
            {
                writer.Write('{');
                expr.WriteError(writer);
                writer.Write('}');
            }
        }

        public void Track(TextWriter writer, Pattern pattern, IInlineExpression exp)
        {
            if (_travelled.Contains(pattern))
            {
                AddError(ResolverFluentError.Cyclic(pattern));
                writer.Write('{');
                exp.WriteError(writer);
                writer.Write('}');
            }
            else
            {
                _travelled.Add(pattern);
                pattern.Write(writer, this);
                PopTraveled();
            }
        }

        private void PopTraveled()
        {
            if (_travelled.Count > 0)
            {
                _travelled.RemoveAt(_travelled.Count - 1);
            }
        }

        public bool WriteRefError(TextWriter writer, IInlineExpression exp)
        {
            AddError(ResolverFluentError.Reference(exp));
            try
            {
                writer.Write('{');
                exp.WriteError(writer);
                writer.Write('}');
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public bool TryGetReference(string argument, [NotNullWhen(true)] out FluentReference? reference)
        {
            if (_args != null && _args.TryGetValue(argument, out var fluentType) && fluentType is FluentReference refs)
            {
                reference = refs;
                return true;
            }

            reference = null;
            return false;
        }

        public ResolvedArgs GetArguments(CallArguments? callArguments)
        {
            var positionalArgs = new List<IFluentType>();
            var namedArgs = new Dictionary<string, IFluentType>();
            if (callArguments != null)
            {
                var listPositional = callArguments.Value.PositionalArgs;
                for (var i = 0; i < listPositional.Count; i++)
                {
                    var expr = listPositional[i].Resolve(this);
                    positionalArgs.Add(expr);
                }

                var listNamed = callArguments.Value.NamedArgs;
                for (var i = 0; i < listNamed.Count; i++)
                {
                    var arg = listNamed[i];
                    namedArgs.Add(arg.Name.ToString(), arg.Value.Resolve(this));
                }
            }

            return new ResolvedArgs(positionalArgs, namedArgs);
        }

        public void SetLocalArgs(IDictionary<string, IFluentType>? resNamed)
        {
            _localNameArgs = resNamed != null
                ? new Dictionary<string, IFluentType>(resNamed)
                : null;
        }

        public void SetLocalArgs(ResolvedArgs resNamed)
        {
            _localNameArgs = (Dictionary<string, IFluentType>?)resNamed.Named;
            _localPosArgs = (List<IFluentType>?)resNamed.Positional;
        }

        public void ClearLocalArgs()
        {
            _localNameArgs = null;
            _localPosArgs = null;
        }

        public PluralCategory GetPluralRules(RuleType type, FluentNumber number)
        {
            return ResolverHelpers.PluralRules.GetPluralCategory(_culture, type, number);
        }

        public string ResolveReference(Identifier refId)
        {
            var refArg = refId.ToString();
            while (true)
            {
                if (IncrPlaceable() && _args != null && _args.TryGetValue(refArg, out var fluentType))
                {
                    if (fluentType is FluentReference dynamicReference)
                    {
                        refArg = dynamicReference.AsString();
                        continue;
                    }

                    return fluentType.AsString();
                }

                return refArg;
            }
        }
    }

    public record ResolvedArgs(IList<IFluentType> Positional, IDictionary<string, IFluentType> Named);
}
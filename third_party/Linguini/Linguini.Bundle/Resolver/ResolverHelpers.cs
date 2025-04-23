using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Shared.Util;
using Linguini.Syntax.Ast;
using PluralRulesGenerated;

namespace Linguini.Bundle.Resolver
{
    public static class ResolverHelpers
    {
        public static IFluentType Resolve(this Pattern self, Scope scope)
        {
            var len = self.Elements.Count;

            if (len == 1)
            {
                if (self.Elements[0] is TextLiteral textLiteral)
                {
                    return GetFluentString(textLiteral.ToString(), scope.TransformFunc);
                }
            }

            var stringWriter = new StringWriter();
            self.Write(stringWriter, scope);
            return (FluentString)stringWriter.ToString();
        }

        public static IFluentType Resolve(this IInlineExpression self, Scope scope, int? pos = null)
        {
            if (self is TextLiteral textLiteral)
            {
                StringWriter stringWriter = new();
                UnicodeUtil.WriteUnescapedUnicode(textLiteral.Value, stringWriter);
                return (FluentString)stringWriter.ToString();
            }

            if (self is NumberLiteral numberLiteral)
            {
                return FluentNumber.TryNumber(numberLiteral.Value.Span);
            }

            if (self is VariableReference varRef)
            {
                var args = scope.LocalNameArgs ?? scope.Args;
                if (args != null
                    && args.TryGetValue(varRef.Id.ToString(), out var arg))
                {
                    return arg.Copy();
                }

                if (scope.LocalPosArgs != null && pos != null
                                               && pos < scope.LocalPosArgs.Count)
                {
                    return scope.LocalPosArgs[pos.Value];
                }

                if (scope.LocalNameArgs == null)
                {
                    scope.AddError(ResolverFluentError.UnknownVariable(varRef));
                }

                return new FluentErrType();
            }

            var writer = new StringWriter();
            self.TryWrite(writer, scope);
            return (FluentString)writer.ToString();
        }

        private static FluentString GetFluentString(string str, Func<string, string>? transformFunc)
        {
            return transformFunc == null ? str : transformFunc(str);
        }

        public static class PluralRules
        {
            public static PluralCategory GetPluralCategory(CultureInfo info, RuleType ruleType, FluentNumber number)
            {
                var specialCase = IsSpecialCase(info.Name, ruleType);
                var langStr = GetPluralRuleLang(info, specialCase);
                var func = RuleTable.GetPluralFunc(langStr, ruleType);
                if (number.TryPluralOperands(out var op))
                {
                    return func(op);
                }

                return PluralCategory.Other;
            }

            public static bool IsSpecialCase(string info, RuleType ruleType)
            {
                if (info.Length < 4)
                    return false;

                var specialCaseTable = ruleType switch
                {
                    RuleType.Ordinal => RuleTable.SpecialCaseOrdinal,
                    _ => RuleTable.SpecialCaseCardinal
                };
                return specialCaseTable.Contains(info);
            }

            private static string GetPluralRuleLang(CultureInfo info, bool specialCase)
            {
                if (CultureInfo.InvariantCulture.Equals(info))
                {
                    // When culture info is uncertain we default to common 
                    // language behavior
                    return "root";
                }

                var langStr = specialCase
                    ? info.Name.Replace('-', '_')
                    : info.TwoLetterISOLanguageName;
                return langStr;
            }
        }
    }
}
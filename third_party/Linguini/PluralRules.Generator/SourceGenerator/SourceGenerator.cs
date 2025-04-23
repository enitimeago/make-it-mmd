using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PluralRules.Generator.Cldr;

namespace PluralRules.Generator.SourceGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private List<CldrRule> _ordinalRules = new();
        private List<CldrRule> _cardinalRules = new();

        public void Initialize(GeneratorInitializationContext context)
        {
            // Not needed
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var myFiles = context.AdditionalFiles.Select(a => a.Path)
                .Where(path => path.EndsWith(".xml"));

            foreach (var path in myFiles)
            {
                if (path.EndsWith("plurals.xml"))
                {
                    _cardinalRules = ProcessXmlPath(path);
                }
                else if (path.EndsWith("ordinals.xml"))
                {
                    _ordinalRules = ProcessXmlPath(path);
                }
            }

            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder($@"
using System;
using Linguini.Shared.Util;
using Linguini.Shared.Types;

namespace PluralRulesGenerated
{{
    public static class RuleTable
    {{
        private static Func<PluralOperands, PluralCategory>[] cardinalMap = 
        {{");
            WriteRules(sourceBuilder, _cardinalRules);
            sourceBuilder.Append($@"
        }};

        private static Func<PluralOperands, PluralCategory>[] ordinalMap = 
        {{");
            WriteRules(sourceBuilder, _ordinalRules);
            sourceBuilder.Append($@"
        }};
        
        private static int GetCardinalIndex(string culture)
        {{");
            WriteRulesIndex(sourceBuilder, _cardinalRules);
            sourceBuilder.Append($@"
        }}

        private static int GetOrdinalIndex(string culture)
        {{");
            WriteRulesIndex(sourceBuilder, _ordinalRules);
            sourceBuilder.Append($@"
        }}

        public static Func<PluralOperands, PluralCategory> GetPluralFunc(string culture, RuleType type)
        {{
            switch (type)
            {{
                case RuleType.Cardinal:
                    return cardinalMap[GetCardinalIndex(culture)];
                case RuleType.Ordinal:
                    return ordinalMap[GetOrdinalIndex(culture)];
            }}

            return _ => PluralCategory.Other;
        }}

        public static string[] SpecialCaseCardinal = {{");
            WriteSpecialCase(sourceBuilder, _cardinalRules);
            sourceBuilder.Append($@"
        }};

        public static string[] SpecialCaseOrdinal = {{");
            WriteSpecialCase(sourceBuilder, _ordinalRules);
            sourceBuilder.Append(@"
        };
        
    }
}
");
            var testBuilder = new StringBuilder($@"
using System;
using Linguini.Shared.Types;

namespace PluralRulesGenerated.Test
{{
    public static class RuleTableTest
    {{
        public static readonly object?[][] CardinalTestData = 
        {{
");
            foreach (var cardinalRule in _cardinalRules)
            {
                WriteRuleTest(testBuilder, cardinalRule, true);
            }

            testBuilder.Append($@"
        }};

        public static readonly object?[][] OrdinalTestData = 
        {{
");
            foreach (var ordinalRule in _ordinalRules)
            {
                WriteRuleTest(testBuilder, ordinalRule, false);
            }

            testBuilder.Append($@"
        }};
    }}
}}");

            context.AddSource("PluralRule", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            context.AddSource("PluralRuleTestTable", SourceText.From(testBuilder.ToString(), Encoding.UTF8));
        }

        private void WriteSpecialCase(StringBuilder sourceBuilder, List<CldrRule> cldrRules)
        {
            List<string> specialCases = new();
            foreach (var cldr in cldrRules)
            {
                foreach (var langId in cldr.LangIds)
                {
                    if (langId.Length > 4)
                    {
                        var specCase = langId.Replace('_', '-');
                        specialCases.Add(langId);
                        specialCases.Add(specCase);
                    }
                }
            }

            if (specialCases.Count > 0)
            {
                foreach (var specialCase in specialCases)
                {
                    sourceBuilder.Append($"\n            \"{specialCase}\",");
                }
            }
        }

        private void WriteRuleTest(StringBuilder testBuilder, CldrRule rule, bool isCardinal)
        {
            string ruleType = isCardinal ? "RuleType.Cardinal" : "RuleType.Ordinal";
            foreach (var ruleLangId in rule.LangIds)
            {
                foreach (var ruleMap in rule.Rules)
                {
                    if (ruleMap.Rule.Samples != null)
                    {
                        string category = $"PluralCategory.{ruleMap.Category.FirstCharToUpper()}";
                        foreach (var integerSample in ruleMap.Rule.Samples.IntegerSamples)
                        {
                            var upper = integerSample.Upper == null
                                ? "null"
                                : $"\"{integerSample.Upper.Value}\"";
                            testBuilder.Append($@"
            new object?[] {{""{ruleLangId}"", {ruleType}, false, ""{integerSample.Lower.Value}"", {upper}, {category}}},");
                        }

                        foreach (var decimalSample in ruleMap.Rule.Samples.DecimalSamples)
                        {
                            var upper = decimalSample.Upper == null
                                ? "null"
                                : $"\"{decimalSample.Upper.Value}\"";
                            testBuilder.Append($@"
            new object?[] {{""{ruleLangId}"", {ruleType}, true, ""{decimalSample.Lower.Value}"", {upper}, {category}}},");
                        }
                    }
                }
            }
        }

        private void WriteRules(StringBuilder stringBuilder, List<CldrRule> rules)
        {
            foreach (var rule in rules)
            {
                if (rule.Rules.Count == 1)
                {
                    var category = rule.Rules[0].Category.FirstCharToUpper();
                    stringBuilder.Append(@$"
            _ => PluralCategory.{category},");
                }
                else
                {
                    WriteRuleMaps(stringBuilder, rule.Rules);
                }
            }
        }

        private void WriteRuleMaps(StringBuilder stringBuilder, List<RuleMap> ruleMaps)
        {
            stringBuilder.Append(@"
            po =>
            {");
            foreach (var ruleMap in ruleMaps)
            {
                WriteRuleMap(stringBuilder, ruleMap);
            }

            stringBuilder.Append(@"
            },");
        }

        private void WriteRuleMap(StringBuilder stringBuilder, RuleMap ruleMap)
        {
            stringBuilder.Append(@"
                ");
            if (ruleMap.Rule.Condition.IsAny())
            {
                stringBuilder.Append($"return PluralCategory.{ruleMap.Category.FirstCharToUpper()};");
            }
            else
            {
                stringBuilder.Append("if (");
                WriteCondition(stringBuilder, ruleMap.Rule.Condition);
                stringBuilder.Append(")");
                stringBuilder.Append($@"
                {{
                    return PluralCategory.{ruleMap.Category.FirstCharToUpper()};
                }}");
            }
        }

        private void WriteCondition(StringBuilder stringBuilder, Condition ruleCondition)
        {
            if (ruleCondition.Conditions.Count < 1)
            {
                stringBuilder.Append("true");
                return;
            }

            for (var i = 0; i < ruleCondition.Conditions.Count; i++)
            {
                if (i != 0)
                {
                    stringBuilder.Append(" || ");
                }

                var andCondition = ruleCondition.Conditions[i]!;
                if (andCondition.Relations.Count == 1)
                {
                    WriteRelation(stringBuilder, andCondition.Relations[0]);
                }
                else
                {
                    for (var j = 0; j < andCondition.Relations.Count; j++)
                    {
                        if (j != 0)
                        {
                            stringBuilder.Append(" && ");
                        }

                        WriteRelation(stringBuilder, andCondition.Relations[j]);
                    }
                }
            }
        }

        private void WriteRelation(StringBuilder stringBuilder, Relation relation)
        {
            var operand = relation.Expr.Operand.ToUpperChar();
            if (operand == "E")
            {
                operand = "Exp()";
            }

            var brackets = relation.RangeListItems.Count > 1;
            if (relation.Expr.Modulus != null)
            {
                var sb = new StringBuilder();

                sb.Append("po.").Append(operand).Append(" % ")
                    .Append(relation.Expr.Modulus.Value);
                if (relation.Op.IsNegated())
                {
                    stringBuilder.Append('!');
                    if (relation.RangeListItems[0] is not RangeElem)
                    {
                        // Have to bracket negation of plain mod
                        // e.g. instead of !po.N % 10 == 0
                        //                 !(po.N % 10 == 0)
                        brackets = true;
                    }
                }

                if (brackets)
                {
                    stringBuilder.Append("(");
                }

                for (var i = 0; i < relation.RangeListItems.Count; i++)
                {
                    if (i != 0)
                    {
                        stringBuilder.AppendLine().Append("                    || ");
                    }

                    var rel = relation.RangeListItems[i]!;
                    if (rel is DecimalValue value)
                    {
                        stringBuilder.Append(sb).Append(" == ").Append(value);
                    }
                    else if (rel is RangeElem rangeElem)
                    {
                        stringBuilder.Append($"({sb}).InRange({rangeElem.LowerVal}, {rangeElem.UpperVal})");
                    }
                }

                if (brackets)
                {
                    stringBuilder.Append(") ");
                }
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append("po.").Append(operand);

                string op = relation.Op.IsNegated() ? " != " : " == ";

                if (brackets)
                {
                    stringBuilder.Append("( ");
                }

                for (var i = 0; i < relation.RangeListItems.Count; i++)
                {
                    if (i != 0)
                    {
                        stringBuilder.Append(" || ").AppendLine().Append("                    ");
                    }

                    var rel = relation.RangeListItems[i]!;
                    if (rel is DecimalValue value)
                    {
                        stringBuilder.Append(sb).Append(op).Append(value);
                    }
                    else if (rel is RangeElem rangeElem)
                    {
                        var negate = relation.Op.IsNegated() ? "!" : "";
                        stringBuilder.Append($"{negate}{sb}.InRange({rangeElem.LowerVal}, {rangeElem.UpperVal}) ");
                    }
                }

                if (brackets)
                {
                    stringBuilder.Append(" )");
                }
            }
        }

        private void WriteRulesIndex(StringBuilder stringBuilder, List<CldrRule> rules)
        {
            stringBuilder.Append(@"
            switch (culture)
            {");
            for (int i = 0; i < rules.Count; i++)
            {
                WriteRuleIndex(stringBuilder, rules[i], i);
            }

            stringBuilder.Append(@"
            }
            return -1;");
        }

        private void WriteRuleIndex(StringBuilder stringBuilder, CldrRule rule, int i)
        {
            foreach (var langId in rule.LangIds)
            {
                stringBuilder.Append($@"
                case ""{langId}"":");
            }

            stringBuilder.Append($@"
                    return {i};");
        }

        private static List<CldrRule> ProcessXmlPath(string? path)
        {
            var rules = new List<CldrRule>();

            if (string.IsNullOrEmpty(path))
            {
                return rules;
            }

            using var cardinalFileStream = File.OpenRead(path!);
            var xmlElems = XDocument.Load(cardinalFileStream)
                .XPathSelectElements("//supplementalData/plurals/*");
            rules = Convert(xmlElems);

            return rules;
        }

        private static List<CldrRule> Convert(IEnumerable<XElement> plurals)
        {
            var retVal = new List<CldrRule>(40);
            foreach (var pluralRule in plurals)
            {
                var langIds = pluralRule.Attribute("locales")!
                    .Value.Split(' ')
                    .ToList();
                var rules = new List<RuleMap>();
                foreach (var element in pluralRule.Elements())
                {
                    var countTag = element.Attribute("count")?.Value;

                    var rule = new CldrParser(element.FirstNode.ToString()).ParseRule();
                    rules.Add(new RuleMap(countTag!, rule));
                }

                retVal.Add(new CldrRule(langIds, rules));
            }

            return retVal;
        }
    }
}

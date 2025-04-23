using System;
using System.Collections.Generic;

namespace PluralRules.Generator.Cldr
{
    
    public class CldrRule
    {
        public List<string> LangIds;
        public List<RuleMap> Rules;

        public CldrRule(List<string> langIds, List<RuleMap> ruleList)
        {
            LangIds = langIds;
            Rules = ruleList;
        }
    }
    
    public class RuleMap
    {
        public string Category;
        public Rule Rule;

        public RuleMap(string category, Rule rule)
        {
            Category = category;
            Rule = rule;
        }
    }
    public class Rule
    {
        public Condition Condition;
        public Samples? Samples;

        public Rule(Condition condition, Samples? samples)
        {
            Condition = condition;
            Samples = samples;
        }
    }

    public class Samples
    {
        public List<SampleRange> IntegerSamples;
        public List<SampleRange> DecimalSamples;

        public Samples(List<SampleRange> integerSamples, List<SampleRange> decimalSamples)
        {
            IntegerSamples = integerSamples;
            DecimalSamples = decimalSamples;
        }
    }

    public class SampleRange
    {
        public DecimalValue Lower;
        public DecimalValue? Upper;

        public SampleRange(DecimalValue lower, DecimalValue? upper)
        {
            Lower = lower;
            Upper = upper;
        }

        public override string ToString()
        {
            if (Upper != null)
            {
                return $"{Lower}~{Upper}";
            }

            return $"{Lower}";
        }
    }

    public class DecimalValue : IRangeListItem, IEquatable<DecimalValue>
    {
        public string Value { get; }

        public DecimalValue(string value)
        {
            Value = value;
        }

        public bool Equals(DecimalValue? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DecimalValue) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }

    public record Condition 
    {
        public List<AndCondition> Conditions;

        public Condition(List<AndCondition> conditions)
        {
            Conditions = conditions;
        }
        public bool IsAny()
        {
            return Conditions.Count == 0;
        }
    }

    public class AndCondition
    {
        public List<Relation> Relations;

        public AndCondition(List<Relation> relations)
        {
            Relations = relations;
        }
    }

    public class Relation
    {
        public Expr Expr;
        public Op Op;
        public List<IRangeListItem> RangeListItems;

        public Relation(Expr expr, Op op, List<IRangeListItem> rangeList)
        {
            Expr = expr;
            Op = op;
            RangeListItems = rangeList;
        }
    }

    public class Expr
    {
        public Operand Operand;
        public DecimalValue? Modulus;
    }

    public enum Operand
    {
        /// <summary>
        /// Absolute value of input
        /// </summary>
        N,

        /// <summary>
        /// Integer value of input
        /// </summary>
        I,

        /// <summary>
        /// Number of visible fractions digits with trailing zeros
        /// </summary>
        V,

        /// <summary>
        /// Number of visible fraction digits without trailing zeros
        /// </summary>
        W,

        /// <summary>
        /// Visible fraction digits with trailing zeros
        /// </summary>
        F,

        /// <summary>
        /// Visible fraction digits without trailing zeros
        /// </summary>
        T,
        
        /// <summary>
        /// Exponent used in some rules
        /// </summary>
        E,
    }

    public static class OperandExtension
    {
        public static Operand? FromChar(char c)
        {
            switch (c)
            {
                case 'n':
                    return Operand.N;
                case 'i':
                    return Operand.I;
                case 'f':
                    return Operand.F;
                case 't':
                    return Operand.T;
                case 'v':
                    return Operand.V;
                case 'w':
                    return Operand.W;
                case 'e':
                    return Operand.E;
                default:
                    return null;
            }
        }

        public static string ToUpperChar(this Operand operand)
        {
            switch (operand)
            {
                case Operand.N:
                    return "N";
                case Operand.I:
                    return "I";
                case Operand.F:
                    return "F";
                case Operand.T:
                    return "T";
                case Operand.V:
                    return "V";
                case Operand.W:
                    return "W";
                case Operand.E:
                    return "E";
                default:
                    throw new ArgumentException($"Unknown operand {operand}"); 
            }
        }
    }


    public interface IRangeListItem
    {
    }

    public class RangeElem : IRangeListItem
    {
        public DecimalValue LowerVal;
        public DecimalValue UpperVal;


        public RangeElem(DecimalValue lowerVal, DecimalValue upperVal)
        {
            LowerVal = lowerVal;
            UpperVal = upperVal;
        }

        public override string ToString()
        {
            return $"{LowerVal}..{UpperVal}";
        }
    }

    public enum Op : byte
    {
        In,
        NotIn,
        Within,
        NotWithin,
        Is,
        IsNot,
        Equal,
        NotEqual,
    }

    public static class OpHelper
    {
        public static bool IsNegated(this Op op)
        {
            switch (op)
            {
                case Op.NotEqual: case Op.NotWithin: case Op.IsNot: case Op.NotIn:
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum RelationType : byte
    {
        Is,
        In,
        Within,
        Equal
    }

    public static class RelationTypeExtensions
    {
        public static Op GetOperator(this RelationType? rt, bool negated)
        {
            if (rt == null)
            {
                throw new ArgumentException("Relation should not be null");
            }

            switch (rt)
            {
                case RelationType.Is:
                    return negated ? Op.IsNot : Op.Is;
                case RelationType.Within:
                    return negated ? Op.NotWithin : Op.Within;
                case RelationType.In:
                    return negated ? Op.NotIn : Op.In;
                case RelationType.Equal:
                    return negated ? Op.NotEqual : Op.Equal;
                default:
                    throw new ArgumentException("Unknown Operator");
            }
        }
    }
}

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
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PluralRules.Generator.Cldr
{
    public class CldrParser
    {
        private readonly string _input;
        private int _pos;

        public CldrParser(string? input)
        {
            _input = input ?? "";
            _pos = 0;
        }

        public Rule ParseRule()
        {
            var condition = ParseCondition();
            var samples = TryParseSamples();

            return new Rule(condition, samples);
        }

        private Samples? TryParseSamples()
        {
            var integerSample = new List<SampleRange>();
            var decimalSample = new List<SampleRange>();
            SkipWhitespace();
            if (TryConsume("@integer"))
            {
                SkipWhitespace();
                integerSample = TryParseSampleList();
            }

            if (TryConsume("@decimal"))
            {
                SkipWhitespace();
                decimalSample = TryParseSampleList();
            }

            if (decimalSample.Count == 0 && integerSample.Count == 0)
            {
                return null;
            }

            return new Samples(integerSample, decimalSample);
        }

        private List<SampleRange> TryParseSampleList()
        {
            var listSample = new List<SampleRange>();

            while (TryParseSampleRange(out var sampleRange, listSample.Count > 0))
            {
                if (sampleRange == null)
                {
                    return listSample;
                }

                listSample.Add(sampleRange);
            }

            TryConsume(',');
            // We ignore the ellipsis in generation
            TryConsume("...");
            TryConsume('…');

            return listSample;
        }

        private bool TryParseSampleRange(out SampleRange? o, bool isNotFirst)
        {
            SkipWhitespace();
            if (isNotFirst)
            {
                if (!TryConsume(','))
                {
                    o = null;
                    return false;
                }

                SkipWhitespace();
            }

            if (!TrySampleValue(out var endValue))
            {
                o = null;
                return false;
            }

            SkipWhitespace();
            if (!TryConsume('~'))
            {
                o = new SampleRange(endValue!, null);
                return true;
            }

            SkipWhitespace();
            if (!TrySampleValue(out var upperVal))
            {
                o = null;
                return false;
            }

            o = new SampleRange(endValue!, upperVal);
            return true;
        }

        private bool TrySampleValue(out DecimalValue? value)
        {
            var x = new StringBuilder();
            if (!TryParseValueAsStr(out var preDot))
            {
                value = null;
                return false;
            }

            x.Append(preDot);
            if (TryConsume('.'))
            {
                if (!TryParseValueAsStr(out var postDot))
                {
                    value = null;
                    return false;
                }

                x.Append('.');
                x.Append(postDot);
            }

            if (!TryConsumeExp(x))
            {
                value = null;
                return false;
            }

            var number = x.ToString();
            if (number.Contains('e'))
            {
                number = Double.Parse(number).ToString(CultureInfo.InvariantCulture);
            }

            value = new DecimalValue(number);
            return true;
        }

        private bool TryConsumeExp(StringBuilder x)
        {
            if (_pos + 1 <= _input.Length)
            {
                if (_input.AsMemory(_pos, 1).Span.IsOneOf('c', 'e'))
                {
                    _pos += 1;
                    x.Append('e');

                    if (TryParseDigitExp(out var digit))
                    {
                        x.Append(digit);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool TryParseDigitExp(out string val)
        {
            var startPos = _pos;
            if (TryPeekCharSpan(out var startDigit)
                && startDigit.IsDigitPos())
            {
                while (TryPeekCharSpan(out var span)
                       && span.IsAsciiDigit())
                {
                    _pos += 1;
                }

                val = _input.Range(startPos, _pos);
                return true;
            }

            val = "";
            return false;
        }

        private Condition ParseCondition()
        {
            var andConditions = new List<AndCondition>();
            SkipWhitespace();
            while (TryParseAndCondition(out var andCondition))
            {
                andConditions.Add(andCondition!);
                SkipWhitespace();
                if (!TryConsume("or"))
                {
                    return new Condition(andConditions);
                }

                SkipWhitespace();
            }

            return new Condition(andConditions);
        }

        private bool TryParseAndCondition(out AndCondition? conditions)
        {
            var relations = new List<Relation>();
            SkipWhitespace();
            while (ParseRelation(out var relation))
            {
                relations.Add(relation!);
                SkipWhitespace();
                if (!TryConsume("and"))
                {
                    break;
                }

                SkipWhitespace();
            }

            conditions = relations.Count > 0 ? new AndCondition(relations) : null;
            return conditions != null;
        }

        private bool ParseRelation(out Relation? relation)
        {
            SkipWhitespace();
            if (!TryParseExpr(out var expr))
            {
                relation = null;
                return false;
            }

            var list = new List<IRangeListItem>();
            RelationType? type = null;
            var negation = false;

            SkipWhitespace();
            if (TryConsume("is"))
            {
                type = RelationType.Is;
                SkipWhitespace();
            }

            if (TryConsume("not"))
            {
                negation = true;
                SkipWhitespace();
            }

            if (TryConsume("within"))
            {
                type = RelationType.Within;
            }
            else if (TryConsume("in"))
            {
                type = RelationType.In;
            }
            else if (TryConsume('='))
            {
                type = RelationType.Equal;
            }
            else if (TryConsume("!="))
            {
                negation = !negation;
                type = RelationType.Equal;
            }

            SkipWhitespace();

            if (type == RelationType.Is)
            {
                if (!TryParseValue(out var x))
                {
                    relation = null;
                    return false;
                }

                list.Add(x);
            }
            else
            {
                if (!TryParseRangeList(out list))
                {
                    relation = null;
                    return false;
                }
            }

            relation = new Relation(expr!, type.GetOperator(negation), list);
            return true;
        }

        private bool TryParseRangeList(out List<IRangeListItem> list)
        {
            list = new List<IRangeListItem>();
            while (TryParseRangeItem(out var x, list.Count > 0))
            {
                list.Add(x!);
            }

            return true;
        }

        private bool TryParseRangeItem(out IRangeListItem? item, bool isNotFirst)
        {
            SkipWhitespace();
            if (isNotFirst)
            {
                if (!TryConsume(","))
                {
                    item = null;
                    return false;
                }

                SkipWhitespace();
            }

            if (!TryParseValue(out var start))
            {
                item = null;
                return false;
            }

            SkipWhitespace();
            if (TryConsume(".."))
            {
                SkipWhitespace();
                if (!TryParseValue(out var end))
                {
                    item = null;
                    return false;
                }

                item = new RangeElem(start, end);
                return true;
            }

            item = start;
            return true;
        }

        private bool TryParseExpr(out Expr? expr)
        {
            SkipWhitespace();
            if (TryOperand(out var operand))
            {
                _pos += 1;
                expr = new Expr { Operand = operand!.Value };

                SkipWhitespace();

                var modulus = ParseModulus();
                expr.Modulus = modulus;
                return true;
            }

            expr = null;
            return false;
        }

        private DecimalValue? ParseModulus()
        {
            if (TryConsume("mod") || TryConsume('%'))
            {
                SkipWhitespace();
                if (TryParseValue(out var val))
                {
                    return val;
                }
            }

            return null;
        }

        private bool TryParseValue(out DecimalValue val)
        {
            var startPos = _pos;
            while (TryPeekCharSpan(out var span)
                   && span.IsAsciiDigit())
            {
                _pos += 1;
            }

            val = new DecimalValue(_input.Range(startPos, _pos));
            return startPos != _pos;
        }

        private bool TryParseValueAsStr(out string val)
        {
            var startPos = _pos;
            while (TryPeekCharSpan(out var span)
                   && span.IsAsciiDigit())
            {
                _pos += 1;
            }

            val = _input.Range(startPos, _pos);
            return startPos != _pos;
        }


        private bool TryPeekCharSpan(out ReadOnlySpan<char> span)
        {
            return _input.AsMemory().TryReadCharSpan(_pos, out span);
        }

        private bool TryOperand(out Operand? operand)
        {
            if (_pos < _input.Length)
            {
                var chr = _input[_pos];
                operand = OperandExtension.FromChar(chr);
                return operand != null;
            }

            operand = null;
            return false;
        }

        private bool TryConsume(string consume)
        {
            if (_pos + consume.Length > _input.Length)
            {
                return false;
            }

            var span = _input.AsMemory(_pos, consume.Length).Span;
            var areEqual = consume.Equals(span.ToString(), StringComparison.InvariantCulture);

            if (areEqual)
            {
                _pos += consume.Length;
            }

            return areEqual;
        }

        private bool TryConsume(char consume)
        {
            if (_pos + 1 > _input.Length)
            {
                return false;
            }

            var span = _input.AsMemory(_pos, 1).Span;
            var areEqual = span.IsEqual(consume);
            if (areEqual)
            {
                _pos += 1;
            }

            return areEqual;
        }

        private void SkipWhitespace()
        {
            while (TryPeekCharSpan(out var span)
                   && span.IsUnicodeWhiteSpace())
            {
                _pos += 1;
            }
        }
    }

    static class SpanUtil
    {
        private static int CharLength = 1;
        private static readonly ReadOnlyMemory<char> Eof = ReadOnlyMemory<char>.Empty;

        public static bool TryReadCharSpan(this ReadOnlyMemory<char> memory, int pos, out ReadOnlySpan<char> span)
        {
            span = Eof.Span;
            if (pos + CharLength > memory.Length)
            {
                return false;
            }

            span = memory.Slice(pos, CharLength).Span;
            return true;
        }

        public static bool IsUnicodeWhiteSpace(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var x = MemoryMarshal.GetReference(charSpan);
            return IsInside(x, '\x09', '\x0D')
                   || x is ' ' or '\u0085' or '\u200E' or '\u200F' or '\u2028' or '\u2029';
        }

        public static bool IsEqual(this ReadOnlySpan<char> charSpan, char c1)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            return MemoryMarshal.GetReference(charSpan) == c1;
        }

        public static bool IsOneOf(this ReadOnlySpan<char> charSpan, char c1, char c2)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var x = MemoryMarshal.GetReference(charSpan);
            return x == c1 || x == c2;
        }


        public static bool IsDigitPos(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, '1', '9');
        }

        public static bool IsAsciiDigit(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, '0', '9');
        }

        private static bool IsInside(char c, char min, char max) => (uint)(c - min) <= (uint)(max - min);
    }

    public static class StringExtensions
    {
        public static string Range(this string input, int start, int end)
        {
            return input.Substring(start, end - start);
        }

        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => "",
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };
    }
}
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
using NUnit.Framework;
using PluralRules.Generator.Cldr;

namespace PluralRules.Test.Cldr
{
    [TestFixture]
    [Parallelizable]
    [TestOf(typeof(CldrParser))]
    public class Tests
    {
        [Test]
        [Parallelizable]
        [TestCase("n is 123", Operand.N, null, Op.Is, new[] {"123"})]
        [TestCase("n is not 456", Operand.N, null, Op.IsNot, new[] {"456"})]
        [TestCase("i% 10 not in 11..15", Operand.I, "10", Op.NotIn, new[] {"11..15"})]
        [TestCase("i % 10 in 131..15", Operand.I, "10", Op.In, new[] {"131..15"})]
        [TestCase("f% 3 = 1", Operand.F, "3", Op.Equal, new[] {"1"})]
        [TestCase("t != 5, 6", Operand.T, null, Op.NotEqual, new[] {"5", "6"})]
        [TestCase("w within 5, 6", Operand.W, null, Op.Within, new[] {"5", "6"})]
        [TestCase("v not within 3, 6..9", Operand.V, null, Op.NotWithin, new[] {"3", "6..9"})]
        [TestCase("n not != 3, 6..9", Operand.N, null, Op.Equal, new[] {"3", "6..9"})]
        [TestCase("n = 1", Operand.N, null, Op.Equal, new[] {"1"})]
        [TestCase("n % 10 in 131..15", Operand.N, "10", Op.In, new[] {"131..15"})]
        [TestCase("e != 0..6", Operand.E, null, Op.NotEqual, new[] {"0..6"})]
        public void BasicParseRule(string input, Operand expOperand, string? modulus, Op expOp,
            string[] expRangeList)
        {
            var rule = new CldrParser(input).ParseRule();
            Assert.That(rule, Is.Not.Null);
            var relation = rule.Condition.Conditions[0].Relations[0];
            Assert.That(expOperand, Is.EqualTo(relation.Expr.Operand));

            if (modulus != null)
            {
                Assert.That(new DecimalValue(modulus), Is.EqualTo(relation.Expr.Modulus));
            }
            else
            {
                Assert.That(relation.Expr.Modulus, Is.Null);
            }

            Assert.That(expOp, Is.EqualTo(relation.Op));
            for (var i = 0; i < expRangeList.Length; i++)
            {
                Assert.That(expRangeList[i], Is.EqualTo(relation.RangeListItems[i].ToString()));
            }
        }

        [Test]
        public void ParseEmpty()
        {
            var rule = new CldrParser("").ParseRule();
            Assert.That(rule, Is.Not.Null);
            Assert.That(rule.Condition.Conditions, Is.Empty);
            Assert.That(rule.Samples, Is.Null);
        }

        [Test]
        [Parallelizable]
        [TestCase("n is 12 @integer 0, 5, 7~20", new[] {"0", "5", "7~20"}, new string[] { })]
        [TestCase("n is 12 @integer 0, 5, 7~20 @decimal 1, 3~6,...", new[] {"0", "5", "7~20"},
            new[] {"1", "3~6"})]
        [TestCase("@integer 0, 11~25, 100, 1000,  …", new[] {"0", "11~25", "100", "1000"}, new string[] { })]
        public void ParseSamples(string input, string[] expIntRangeList, string[] expDecRangeList)
        {
            var rule = new CldrParser(input).ParseRule();
            Assert.That(rule.Samples, Is.Not.Null);
            for (var i = 0; i < expIntRangeList.Length; i++)
            {
                Assert.That(expIntRangeList[i], Is.EqualTo(rule.Samples?.IntegerSamples[i].ToString()));
            }

            for (var i = 0; i < expDecRangeList.Length; i++)
            {
                Assert.That(expDecRangeList[i], Is.EqualTo(rule.Samples?.DecimalSamples[i].ToString()));
            }
        }

        [Test]
        [Parallelizable] 
        [TestCase("v= 0 @integer 1c1, 1c2, 1c3, 1c4, 1c6", new[] {10, 100, 1000, 10000, 1000000}, null)]
        [TestCase("v= 0 @decimal 2c2, 2c3, 2c4, 2c6", null, new[] {200.0, 2000.0, 20000.0, 2000000.0})]
        [TestCase(" @integer 1c1, 1c2, 1c3, 1c4, 1c6 @decimal 3c2, 3c3, 3c4",
            new[] {10, 100, 1000, 10000, 1000000}, new[] {300.0, 3000.0, 30000.0})]
        public void TestParseExponent(string input, int[]? integerRanges, double[]? decimalRanges)
        {
            var rule = new CldrParser(input).ParseRule();
            Assert.That(rule, Is.Not.Null);
            Assert.That(rule.Samples, Is.Not.Null);
            if (integerRanges != null)
            {
                for (var index = 0; index < integerRanges.Length; index++)
                {
                    var expected = integerRanges[index];
                    var ruleStr = rule.Samples?.IntegerSamples[index].Lower.Value;
                    var dec = Double.Parse(ruleStr!);
                    Assert.That(expected, Is.EqualTo(Convert.ToInt32(dec)));
                }
            }

            if (decimalRanges != null)
            {
                for (var index = 0; index < decimalRanges.Length; index++)
                {
                    var expected = decimalRanges[index];
                    var ruleStr = rule.Samples?.DecimalSamples[index].Lower.Value;
                    var actual = Double.Parse(ruleStr!);
                    Assert.That(expected, Is.EqualTo(actual));
                }
            }
        }

        [Test]
        public void ParseCondition()
        {
            var rule = new CldrParser("i = 0 or n = 1 and f = 0 ").ParseRule();
            Assert.That(rule.Condition, Is.Not.Null);
            Assert.That(2, Is.EqualTo(rule.Condition.Conditions.Count));
            Assert.That(1, Is.EqualTo(rule.Condition.Conditions[0].Relations.Count));
            Assert.That(2, Is.EqualTo(rule.Condition.Conditions[1].Relations.Count));
        }
    }
}

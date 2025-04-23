using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;
using PluralRulesGenerated.Test;
using static Linguini.Bundle.Resolver.ResolverHelpers.PluralRules;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    [Parallelizable]
    public class TestRules
    {
        public static readonly object?[][] OrdinalTestData = RuleTableTest.OrdinalTestData;
        public static readonly object?[][] CardinalTestData = RuleTableTest.CardinalTestData;

        [Test]
        [Parallelizable]
        [Category("Cardinals")]
        [TestCaseSource(nameof(CardinalTestData))]
        public void TestCardinal(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, isDecimal, lower, upper, expected);
        }

        [Test]
        [Parallelizable]
        [Category("Ordinals")]
        [TestCaseSource(nameof(OrdinalTestData))]
        public void TestOrdinal(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, isDecimal, lower, upper, expected);
        }

        [Test]
        [Parallelizable]
        [TestCase("root", RuleType.Cardinal, false, "0", "15", PluralCategory.Other)]
        public void TestIndividual(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, isDecimal, lower, upper, expected);
        }


        private static void TestData(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            if (!TryGetCultureInfo(cultureStr, type, out var info))
                return;

            // If upper limit exist, we probe the range a bit
            if (upper != null)
            {
                var start = FluentNumber.FromString(lower);
                var end = FluentNumber.FromString(upper);
                var midDouble = (end.Value - start.Value) / 2 + start;
                FluentNumber mid = isDecimal
                    ? midDouble
                    : Convert.ToInt32(Math.Floor(midDouble), CultureInfo.InvariantCulture);

                var actualStart = GetPluralCategory(info, type, start);
                Assert.That(expected, Is.EqualTo(actualStart), $"Failed on start of range: {start}");
                var actualEnd = GetPluralCategory(info, type, end);
                Assert.That(expected, Is.EqualTo(actualEnd), $"Failed on end of range: {end}");
                var actualMid = GetPluralCategory(info, type, mid);
                Assert.That(expected, Is.EqualTo(actualMid), $"Failed on middle of range: {mid}");
            }
            else
            {
                var value = FluentNumber.FromString(lower);
                var actual = GetPluralCategory(info, type, value);
                Assert.That(expected, Is.EqualTo(actual));
            }
        }

        private static bool TryGetCultureInfo(string cultureStr, RuleType type,
            [NotNullWhen(true)] out CultureInfo? culture)
        {
            if (cultureStr.Equals("root"))
            {
                culture = CultureInfo.InvariantCulture;
                return true;
            }

            try
            {
                var cultureStrInfo = IsSpecialCase(cultureStr, type)
                    ? cultureStr.Replace('_', '-')
                    : cultureStr;
                culture = new CultureInfo(cultureStrInfo);
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            culture = null;
            return false;
        }
    }
}

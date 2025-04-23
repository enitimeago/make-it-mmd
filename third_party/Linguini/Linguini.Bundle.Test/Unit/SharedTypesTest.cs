using Linguini.Shared.Types;
using NUnit.Framework;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    [Parallelizable]
    public class SharedTypesTest
    {
        [Test]
        [Parallelizable]
        [TestCase(null, null)]
        [TestCase("zero", PluralCategory.Zero)]
        [TestCase("one", PluralCategory.One)]
        [TestCase("two", PluralCategory.Two)]
        [TestCase("few", PluralCategory.Few)]
        [TestCase("many", PluralCategory.Many)]
        [TestCase("other", PluralCategory.Other)]
        [TestCase("default", PluralCategory.Other)]
        [TestCase("err", null)]
        public void TestPluralCategoryHelper(string? input, PluralCategory? expected)
        {
            Assert.That(expected != null, Is.EqualTo(input.TryPluralCategory(out var actual)));
            Assert.That(expected, Is.EqualTo(actual));
        }
    }
}

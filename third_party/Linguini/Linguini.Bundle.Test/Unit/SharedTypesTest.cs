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
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types;
using NUnit.Framework;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Test.Unit
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

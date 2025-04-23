using System;
using Linguini.Shared.Util;
using NUnit.Framework;

namespace Linguini.Syntax.Tests.IO
{
    [TestFixture]
    [TestOf(typeof(ZeroCopyUtil))]
    public class NonValidCharInputTest
    {
        [TestCase("string")]
        [TestCase("clear")]
        [TestCase("漢字")]
        [TestCase("단편")]
        [TestCase("かんじ")]
        [TestCase("Северный поток")]
        public void OperationOnNonCalleeReturnFalse(string input)
        {
            var span = input.AsSpan();
            Assert.That(span.IsCallee(), Is.False);
        }
    }
}

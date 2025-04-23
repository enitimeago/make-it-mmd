using System;
using System.IO;
using Linguini.Shared.Util;
using NUnit.Framework;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    [TestOf(typeof(UnicodeUtil))]
    public class TestUnicodeUnescaping
    {
        [Test]
        [Parallelizable]
        [TestCase("foo", "foo")]
        [TestCase("foo \\\\", "foo \\")]
        [TestCase("foo \\\"", "foo \"")]
        [TestCase("foo \\\\ faa", "foo \\ faa")]
        [TestCase("foo \\\\ faa \\\\ fii", "foo \\ faa \\ fii")]
        [TestCase("foo \\\\\\\" faa \\\"\\\\ fii", "foo \\\" faa \"\\ fii")]
        [TestCase("\\u0041\\u004F", "AO")]
        [TestCase("\\uA", "�")]
        [TestCase("\\uA0Pl", "�")]
        [TestCase("\\d Foo", "� Foo")]
        [TestCase("\\U01F602", "😂")]
        public void TestUnescape(string input, string expected)
        {
            StringWriter stringWriter = new();
            UnicodeUtil.WriteUnescapedUnicode(input.AsMemory(), stringWriter);
            
            Assert.That(expected, Is.EqualTo(stringWriter.ToString()));
        }
    }
}

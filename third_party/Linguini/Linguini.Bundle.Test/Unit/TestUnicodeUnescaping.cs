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
using System.IO;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Util;
using NUnit.Framework;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Bundle.Test.Unit
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

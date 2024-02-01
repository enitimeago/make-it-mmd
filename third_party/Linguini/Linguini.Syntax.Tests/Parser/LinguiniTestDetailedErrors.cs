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
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Parser;
using NUnit.Framework;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Tests.Parser
{
    [TestFixture]
    public class LinguiniTestDetailedErrors
    {
        [Test]
        [TestCase("### Comment\nterm1", 2, 12, 17, 17, 18)]
        public void TestDetailedErrors(string input, int row, int startErr, int endErr,
            int startMark, int endMark)
        {
            var parse = new LinguiniParser(input).Parse();
            var parseWithComments = new LinguiniParser(input).ParseWithComments();

            Assert.That(1, Is.EqualTo(parse.Errors.Count));
            Assert.That(1, Is.EqualTo(parseWithComments.Errors.Count));

            var detailMsg = parse.Errors[0];
            Assert.That(row, Is.EqualTo(detailMsg.Row));
            Assert.That(detailMsg.Slice, Is.Not.Null);
            Assert.That(new Range(startErr, endErr), Is.EqualTo(detailMsg.Slice!.Value));
            Assert.That(new Range(startMark, endMark), Is.EqualTo(detailMsg.Position));
        }

        [Test]
        public void TestLineOffset()
        {
            const string code = @"a = b
c = d

foo = {

d = e
";

            var parser = new LinguiniParser(code);
            var result = parser.Parse();
            var error = result.Errors[0];

            Assert.That(error.Row, Is.EqualTo(6));
        }
    }
}
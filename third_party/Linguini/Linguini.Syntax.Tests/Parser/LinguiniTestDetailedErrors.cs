using System;
using Linguini.Syntax.Parser;
using NUnit.Framework;

namespace Linguini.Syntax.Tests.Parser
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
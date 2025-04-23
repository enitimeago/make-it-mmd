using System;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;
using Linguini.Syntax.Parser.Error;
using NUnit.Framework;

#pragma warning disable 8602
#pragma warning disable 8600

namespace Linguini.Syntax.Tests.Parser
{
    [Parallelizable]
    [TestFixture]
    [TestOf(typeof(LinguiniParser))]
    public class LinguiniParserTest
    {
        #region CommentTest

        [Test]
        [Parallelizable]
        [TestCase("# Comment")]
        [TestCase("## Comment", CommentLevel.GroupComment)]
        [TestCase("### Comment", CommentLevel.ResourceComment)]
        [TestCase("### Funny funny", CommentLevel.ResourceComment, "Funny funny")]
        [TestCase("###  漢字", CommentLevel.ResourceComment, " 漢字")]
        [TestCase("# Comment\r\n")]
        [TestCase("# Comment\n")]
        public void TestCommentParse(string input, CommentLevel expectedCommentLevel = CommentLevel.Comment,
            string expectedContent = "Comment")
        {
            Resource parsed = new LinguiniParser(input).ParseWithComments();
            Assert.That(parsed.Entries.Count, Is.EqualTo(1));
            if (parsed.Entries[0] is AstComment comment)
            {
                Assert.That(expectedCommentLevel, Is.EqualTo(comment.CommentLevel));
                Assert.That(expectedContent, Is.EqualTo(comment.AsStr()));
            }
            else
            {
                Assert.Fail("Comment was not found");
            }
        }

        [Test]
        [Parallelizable]
        [TestCase("#Comment", ErrorType.ExpectedToken,
            "Expected a token starting with  \" \" found \"C\" instead", 1, 2, 0, 8)]
        [TestCase("#Comment\n", ErrorType.ExpectedToken,
            "Expected a token starting with  \" \" found \"C\" instead", 1, 2, 0, 9)]
        [TestCase("#Comment\r\n", ErrorType.ExpectedToken,
            "Expected a token starting with  \" \" found \"C\" instead", 1, 2, 0, 10)]
        public void TestErrorCommentParse(string input, ErrorType expErrType, string expMsg, int start, int end,
            int sliceStart, int sliceEnd)
        {
            Resource parsed = new LinguiniParser(input).ParseWithComments();
            Assert.That(parsed.Errors.Count, Is.EqualTo(1));
            Assert.That(expErrType, Is.EqualTo(parsed.Errors[0].Kind));
            Assert.That(expMsg, Is.EqualTo(parsed.Errors[0].Message));
            Assert.That(new Range(start, end), Is.EqualTo(parsed.Errors[0].Position));
            Assert.That(new Range(sliceStart, sliceEnd), Is.EqualTo(parsed.Errors[0].Slice));
        }

        #endregion

        #region MessageTest

        [Test]
        [Parallelizable]
        [TestCase("a = b", "a", "b")]
        [TestCase("a = \"b\"", "a", "\"b\"")]
        [TestCase("# comment\na = \"b\"", "a", "\"b\"")]
        [TestCase("hello = wo\n rld", "hello", "wo\nrld")]
        [TestCase("a = test\n  test", "a", "test\ntest")]
        [TestCase("a = test\r\n  test", "a", "test\ntest")]
        [TestCase("hello = \n  world", "hello", "world")]
        [TestCase("a = \ttest", "a", "\ttest")]
        [TestCase("a=\n\n  bar\n  baz", "a", "bar\nbaz")]
        public void TestMessageParse(string input, string expName, string expValue)
        {
            Resource parsed = new LinguiniParser(input).ParseWithComments();
            Assert.That(0, Is.EqualTo(parsed.Errors.Count), "Failed, with errors");
            Assert.That(1, Is.EqualTo(parsed.Entries.Count));
            if (parsed.Entries[0] is AstMessage { Value: { } } message)
            {
                Assert.That(expName, Is.EqualTo(message.Id.ToString()));
                Assert.That(expValue, Is.EqualTo(message.Value.Stringify()));
            }
            else
            {
                Assert.Fail("Failed to parse");
            }
        }

        [Test]
        [Parallelizable]
        [TestCase("# comment\na = b", true, "a", "comment")]
        [TestCase("## comment\nhello = world", false, "hello", "comment")]
        [TestCase("# Msg Comment\n# with blank line.\n#\nhello = term",
            true, "hello", "Msg Comment\nwith blank line.\n")]
        public void TestMessageComment(string input, bool inMessage, string expMsg, string expComment)
        {
            var expBodySize = inMessage ? 1 : 2;
            Resource parsed = new LinguiniParser(input).ParseWithComments();
            Assert.That(parsed.Errors.Count, Is.EqualTo(0));
            Assert.That(parsed.Entries.Count, Is.EqualTo(expBodySize));
            if (inMessage)
            {
                if (parsed.Entries[0] is AstMessage msg)
                {
                    Assert.That(expMsg, Is.EqualTo(new string(msg.Id.Name.ToArray())));
                    Assert.That(expComment, Is.EqualTo(msg.Comment.AsStr()));
                }
                else
                {
                    Assert.Fail("No AstMessage found");
                }
            }
            else
            {
                if (parsed.Entries[0] is AstComment comment
                    && parsed.Entries[1] is AstMessage message)
                {
                    Assert.That(expComment, Is.EqualTo(comment.AsStr()));
                    Assert.That(expMsg, Is.EqualTo(new string(message.Id.Name.ToArray())));
                }
                else
                {
                    Assert.Fail($"Unexpected values ${parsed.Entries[0]} and ${parsed.Entries[1]}");
                }
            }
        }

        #endregion

        [Test]
        [TestCase("# Term\r\n# blank line.\r\n#\r\n-term = Term", true, "term", "Term\nblank line.\n")]
        public void TestTermComment(string input, bool inTerm, string expTerm, string expComment)

        {
            var expBodySize = inTerm ? 1 : 2;
            Resource parsed = new LinguiniParser(input).ParseWithComments();
            Assert.That(0, Is.EqualTo(parsed.Errors.Count));
            Assert.That(expBodySize, Is.EqualTo(parsed.Entries.Count));
            if (inTerm)
            {
                if (parsed.Entries[0] is AstTerm term)
                {
                    Assert.That(expTerm, Is.EqualTo(new string(term.Id.Name.ToArray())));
                    Assert.That(expComment, Is.EqualTo(term.Comment.AsStr()));
                }
                else
                {
                    Assert.Fail($"Expected term, found {parsed.Entries[0]}");
                }
            }
            else
            {
                if (parsed.Entries[0] is AstComment comment
                    && parsed.Entries[1] is AstTerm term)
                {
                    Assert.That(expComment, Is.EqualTo(comment.AsStr()));
                    Assert.That(expTerm, Is.EqualTo(new string(term.Id.Name.ToArray())));
                }
                else
                {
                    Assert.Fail($"Expected term, found {parsed.Entries[0]}");
                }
            }
        }

        [Test]
        [Parallelizable]
        [TestCase("num = {-3.14}", "num", "-3.14")]
        [TestCase("num = {123}", "num", "123")]
        public void TestNumExpressions(string input, string identifier, string value)
        {
            var res = new LinguiniParser(input).Parse();

            Assert.That(0, Is.EqualTo(res.Errors.Count));
            Assert.That(1, Is.EqualTo(res.Entries.Count));
            Assert.That(res.Entries[0], Is.InstanceOf<AstMessage>());
            
            if (res.Entries[0] is not AstMessage message
                || message.Value.Elements[0] is not Placeable placeable
                || placeable.Expression is not NumberLiteral numberLiteral) return;
            
            Assert.That(1, Is.EqualTo(message.Value.Elements.Count));
            Assert.That(message.Value.Elements[0], Is.InstanceOf<Placeable>());
            Assert.That(placeable, Is.Not.Null);
            Assert.That(placeable.Expression, Is.InstanceOf<NumberLiteral>());
            Assert.That(numberLiteral, Is.Not.Null);
            Assert.That(identifier, Is.EqualTo(message.Id.ToString()));
            Assert.That(value, Is.EqualTo(numberLiteral.ToString()));
        }

        private const string CrlfEscape = "message = \r\n" +
                                          "  Line1\r\n" +
                                          "  \r\n" +
                                          "  \r\n" +
                                          "  Line2.\r\n";

        private const string CrEscape = "message = \n" +
                                        "  Line1\n" +
                                        "\n" +
                                        "\n" +
                                        "  Line2.\n";

        private const string Expected = "Line1\n\n\nLine2.";

        [Test]
        [TestCase(CrlfEscape)]
        // [TestCase(CrEscape)]
        public void TestNewlinePreservation(string input)
        {
            var parse = new LinguiniParser(input).Parse();

            Assert.That(0, Is.EqualTo(parse.Errors.Count));
            var msg = parse.Entries[0];
            Assert.That(msg, Is.InstanceOf<AstMessage>());
            var astMsg = (AstMessage)msg;
            Assert.That(Expected, Is.EqualTo(astMsg.Debug()));
        }
    }
}
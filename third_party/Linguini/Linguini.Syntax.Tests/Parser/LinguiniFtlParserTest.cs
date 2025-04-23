using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions.Json;
using Linguini.Serialization.Converters;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Linguini.Syntax.Tests.Parser
{
    [TestFixture]
    public class LinguiniFtlParserTest
    {

        private static string _baseTestDir = "";

        private static string BaseTestDir
        {
            get
            {
                if (_baseTestDir == "")
                {
                    // We discard the last three folders from WorkDirectory
                    // to get into common test directory
                    _baseTestDir = Path.GetFullPath(
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", ".."));
                }

                return _baseTestDir;
            }
        }

        private static JsonSerializerOptions TestJsonOptions =
            new()
            {
                IgnoreReadOnlyFields = false,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new AttributeSerializer(),
                    new CallArgumentsSerializer(),
                    new CommentSerializer(),
                    new FunctionReferenceSerializer(),
                    new IdentifierSerializer(),
                    new JunkSerializer(),
                    new MessageReferenceSerializer(),
                    new MessageSerializer(),
                    new DynamicReferenceSerializer(),
                    new NamedArgumentSerializer(),
                    new ParseErrorSerializer(),
                    new PatternSerializer(),
                    new PlaceableSerializer(),
                    new ResourceSerializer(),
                    new PlaceableSerializer(),
                    new SelectExpressionSerializer(),
                    new TermReferenceSerializer(),
                    new TermSerializer(),
                    new VariantSerializer(),
                    new VariableReferenceSerializer(),
                },
            };

        private static string GetFullPathFor(string file)
        {
            List<string> list = new();
            list.Add(BaseTestDir);
            list.AddRange(file.Split('/'));
            return Path.Combine(list.ToArray());
        }


        private static Resource ParseFtlFile(string path, bool enableExtensions = false)
        {
            LinguiniParser parser;
            using (var reader = new StreamReader(path))
            {
                parser = new LinguiniParser(reader, enableExtensions);
            }

            return parser.ParseWithComments();
        }

        private static Resource ParseFtlFileFast(string path, bool enableExtensions = false)
        {
            LinguiniParser parser;
            using (var reader = new StreamReader(path))
            {
                parser = new LinguiniParser(reader, enableExtensions);
            }

            return parser.Parse();
        }


        [Test]
        [Parallelizable]
        [TestCase("fixtures_errors/func", true)]
        [TestCase("fixtures_errors/func", false)]
        [TestCase("fixtures_errors/wrong_row", true)]
        [TestCase("fixtures_errors/wrong_row", false)]
        public void TestLinguiniErrors(string file, bool ignoreComments = false)
        {
            var path = GetFullPathFor(file);
            var expected = WrapArray(JArray.Parse(File.ReadAllText($@"{path}.json")));
            var resource = ignoreComments
                ? ParseFtlFileFast(@$"{path}.ftl")
                : ParseFtlFile(@$"{path}.ftl");

            var actual = WrapArray(JArray.Parse(JsonSerializer.Serialize(resource.Errors, TestJsonOptions)));
            actual.Should().BeEquivalentTo(expected);
        }

        private JToken WrapArray(JArray array)
        {
            JContainer newToken = new JObject();
            newToken.Add(new JProperty("errors", array));

            return newToken;
        }

        [Test]
        [Parallelizable]
        [TestCase("fixtures/any_char")]
        [TestCase("fixtures/astral")]
        [TestCase("fixtures/call_expressions")]
        [TestCase("fixtures/callee_expressions")]
        [TestCase("fixtures/comments")]
        [TestCase("fixtures/cr")]
        [TestCase("fixtures/crlf")]
        [TestCase("fixtures/eof_comment")]
        [TestCase("fixtures/eof_empty")]
        [TestCase("fixtures/eof_id")]
        [TestCase("fixtures/eof_id_equals")]
        [TestCase("fixtures/eof_junk")]
        [TestCase("fixtures/eof_value")]
        [TestCase("fixtures/escaped_characters")]
        [TestCase("fixtures/junk")]
        [TestCase("fixtures/leading_dots")]
        [TestCase("fixtures/literal_expressions")]
        [TestCase("fixtures/member_expressions")]
        [TestCase("fixtures/messages")]
        [TestCase("fixtures/mixed_entries")]
        [TestCase("fixtures/multiline_values")]
        [TestCase("fixtures/numbers")]
        [TestCase("fixtures/obsolete")]
        [TestCase("fixtures/placeables")]
        [TestCase("fixtures/reference_expressions")]
        [TestCase("fixtures/select_expressions")]
        [TestCase("fixtures/select_indent")]
        [TestCase("fixtures/sparse_entries")]
        [TestCase("fixtures/special_chars")]
        [TestCase("fixtures/tab")]
        [TestCase("fixtures/term_parameters")]
        [TestCase("fixtures/terms")]
        [TestCase("fixtures/variables")]
        [TestCase("fixtures/variant_keys")]
        [TestCase("fixtures/whitespace_in_value")]
        [TestCase("fixtures/zero_length")]
        public void TestReadFile(string file)
        {
            var path = GetFullPathFor(file);
            var res = ParseFtlFile(@$"{path}.ftl");
            var ftlAstJson = JsonSerializer.Serialize(res, TestJsonOptions);

            var expected = JToken.Parse(File.ReadAllText($@"{path}.json"));
            var actual = JToken.Parse(ftlAstJson);
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        [Parallelizable]
        [TestCase("fixtures/any_char")]
        [TestCase("fixtures/astral")]
        [TestCase("fixtures/call_expressions")]
        [TestCase("fixtures/callee_expressions")]
        [TestCase("fixtures/comments")]
        [TestCase("fixtures/cr")]
        [TestCase("fixtures/crlf")]
        [TestCase("fixtures/eof_comment")]
        [TestCase("fixtures/eof_empty")]
        [TestCase("fixtures/eof_id")]
        [TestCase("fixtures/eof_id_equals")]
        [TestCase("fixtures/eof_junk")]
        [TestCase("fixtures/eof_value")]
        [TestCase("fixtures/escaped_characters")]
        [TestCase("fixtures/junk")]
        [TestCase("fixtures/leading_dots")]
        [TestCase("fixtures/literal_expressions")]
        [TestCase("fixtures/member_expressions")]
        [TestCase("fixtures/messages")]
        [TestCase("fixtures/mixed_entries")]
        [TestCase("fixtures/multiline_values")]
        [TestCase("fixtures/numbers")]
        [TestCase("fixtures/obsolete")]
        [TestCase("fixtures/placeables")]
        [TestCase("fixtures/reference_expressions")]
        [TestCase("fixtures/select_expressions")]
        [TestCase("fixtures/select_indent")]
        [TestCase("fixtures/sparse_entries")]
        [TestCase("fixtures/special_chars")]
        [TestCase("fixtures/tab")]
        [TestCase("fixtures/term_parameters")]
        [TestCase("fixtures/terms")]
        // [TestCase("fixtures/variables")] Illegal because one error is promoted to syntax.
        [TestCase("fixtures/variant_keys")]
        [TestCase("fixtures/whitespace_in_value")]
        [TestCase("fixtures/zero_length")]
        [TestCase("fixtures_ext/x_linguini_ref")]
        [TestCase("fixtures_ext/x_linguini_term_ref")]
        [TestCase("fixtures_ext/x_linguini_ref_attr")]
        public void TestLinguiniExt(string file)
        {
            var path = GetFullPathFor(file);
            var res = ParseFtlFile(@$"{path}.ftl", true);
            var ftlAstJson = JsonSerializer.Serialize(res, TestJsonOptions);
        
            var expected = JToken.Parse(File.ReadAllText($@"{path}.json"));
            var actual = JToken.Parse(ftlAstJson);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
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
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Serialization.Converters;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;
using NUnit.Framework;
using Attribute = enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast.Attribute;


namespace Linguini.Serialization.Test;

[TestFixture]
public class SerializeAndDeserializeTest
{
    [Test]
    [TestCaseSource(nameof(AstExamples))]
    [Parallelizable]
    public void RoundTripTest(object x)
    {
        // Serialize the object to JSON string.
        var jsonString = JsonSerializer.Serialize(x, Options);

        // Deserialize the JSON string back into an object.
        Debug.Assert(x != null, nameof(x) + " != null");
        var deserializedObject = JsonSerializer.Deserialize(jsonString, x.GetType(), Options);

        // Now you have a 'deserializedObject' which should be equivalent to the original 'expected' object.
        Assert.That(deserializedObject, Is.Not.Null);
        Assert.That(deserializedObject, Is.EqualTo(x));
    }

    public static IEnumerable<object> AstExamples()
    {
        yield return new Attribute("desc", new PatternBuilder("description"));
        yield return new CallArgumentsBuilder()
            .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
            .AddNamedArg("y", 3)
            .Build();
        yield return new AstComment(CommentLevel.Comment, new() { "test".AsMemory() });
        yield return new DynamicReference("dyn", "attr", new CallArgumentsBuilder()
            .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
            .AddNamedArg("y", 3));
        yield return new FunctionReference("foo", new CallArgumentsBuilder()
            .AddPositionalArg(3)
            .AddNamedArg("test", InlineExpressionBuilder.CreateTermReference("x", "y"))
            .Build()
        );
        yield return new Identifier("test");
        yield return new Junk("Test".AsMemory());
        yield return new MessageReference("message", "attribute");
        yield return new AstMessage(
            new Identifier("x"), 
            new PatternBuilder(3).Build(), 
            new List<Attribute>()
            {
                new("attr1", new PatternBuilder("value1")),
                new("attr2", new PatternBuilder("value2"))
            }, 
            new(CommentLevel.ResourceComment, new()
            {
                "test".AsMemory()
            }));
        yield return new PatternBuilder("text ").AddMessage("x").AddText(" more text").Build();
        yield return new SelectExpressionBuilder(new VariableReference("x"))
            .AddVariant("one", new PatternBuilder("select 1"))
            .AddVariant("other", new PatternBuilder("select other"))
            .SetDefault(1)
            .Build();
        yield return new TermReference("x", "y");
        yield return new VariableReference("x");
        yield return new Variant(2.0f, new PatternBuilder(3));
    }

    private static readonly JsonSerializerOptions Options = new()
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
        }
    };
}
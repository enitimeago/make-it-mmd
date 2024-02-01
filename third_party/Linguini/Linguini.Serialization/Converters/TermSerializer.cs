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
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class TermSerializer : JsonConverter<AstTerm>
    {
        public override AstTerm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AstTerm term, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Term");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, term.Id, options);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, term.Value, options);

            writer.WritePropertyName("attributes");
            writer.WriteStartArray();
            foreach (var attribute in term.Attributes)
            {
                JsonSerializer.Serialize(writer, attribute, options);
            }

            writer.WriteEndArray();


            if (term.Comment != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("comment");
                JsonSerializer.Serialize(writer, term.Comment, options);
            }

            writer.WriteEndObject();
        }
    }
}

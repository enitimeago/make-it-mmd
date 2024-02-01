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
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;
using Attribute = enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast.Attribute;

namespace Linguini.Serialization.Converters
{
    public class MessageSerializer : JsonConverter<AstMessage>
    {
        public override AstMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            if (!el.TryGetProperty("id", out var jsonId) ||
                !IdentifierSerializer.TryGetIdentifier(jsonId, options, out var identifier))
            {
                throw new JsonException("AstMessage must have at least `id` element");
            }

            Pattern? value = null;
            AstComment? comment = null;
            var attrs = new List<Attribute>();
            if (el.TryGetProperty("value", out var patternJson) && patternJson.ValueKind == JsonValueKind.Object)
            {
                PatternSerializer.TryReadPattern(patternJson, options, out value);
            }

            if (el.TryGetProperty("comment", out var commentJson) && patternJson.ValueKind == JsonValueKind.Object)
            {
                comment = JsonSerializer.Deserialize<AstComment>(commentJson.GetRawText(), options);
            }

            if (el.TryGetProperty("attributes", out var attrsJson) && attrsJson.ValueKind == JsonValueKind.Array)
            {
                foreach (var attributeJson in attrsJson.EnumerateArray())
                {
                    var attr = JsonSerializer.Deserialize<Attribute>(attributeJson.GetRawText(), options);
                    if (attr != null) attrs.Add(attr);
                }
            }

            return new AstMessage(identifier, value, attrs, comment);
        }

        public override void Write(Utf8JsonWriter writer, AstMessage msg, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteStringValue("Message");


            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, msg.Id, options);

            if (msg.Value != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, msg.Value, options);
            }

            writer.WritePropertyName("attributes");
            writer.WriteStartArray();
            foreach (var attribute in msg.Attributes)
            {
                JsonSerializer.Serialize(writer, attribute, options);
            }

            writer.WriteEndArray();


            if (msg.Comment != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("comment");
                JsonSerializer.Serialize(writer, msg.Comment, options);
            }

            writer.WriteEndObject();
        }
    }
}
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
using Attribute = enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast.Attribute;

namespace Linguini.Serialization.Converters
{
    public class AttributeSerializer : JsonConverter<Attribute>
    {
        public override Attribute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var id = new Identifier("");
            var value = new Pattern();
            
            while (reader.Read())
            {

                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();

                    reader.Read(); 

                    switch (propertyName)
                    {
                        case "id":
                            id = JsonSerializer.Deserialize<Identifier>(ref reader, options); 
                            break;

                        case "value":
                            value = JsonSerializer.Deserialize<Pattern>(ref reader, options); 
                            break;
                        case "type":
                            var typeField = reader.GetString();
                            if (typeField != "Attribute")
                            {
                                throw new JsonException(
                                    $"Invalid type: Expected 'Attribute' found {typeField} instead");
                            }
                            break;
                        default:
                            throw new JsonException($"Unexpected property: {propertyName}");
                    }
                }
            }

            return new Attribute(id!, value!);
        }

        public override void Write(Utf8JsonWriter writer, Attribute attribute, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Attribute");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, attribute.Id, options);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, attribute.Value, options);
            writer.WriteEndObject();
        }
    }
}

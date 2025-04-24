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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters

{
    public class IdentifierSerializer : JsonConverter<Identifier>
    {
        public override Identifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            string? id = null;

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
                        case "type":
                            var typeField = reader.GetString();
                            if (typeField != "Identifier")
                            {
                                throw new JsonException(
                                    $"Invalid type: Expected 'Attribute' found {typeField} instead");
                            }

                            break;
                        case "name":
                            id = reader.GetString();
                            break;
                        default:
                            throw new JsonException($"Unexpected property: {propertyName}");
                    }
                }
            }

            if (id == null)
            {
                throw new JsonException("No id for Identifier found");
            }

            return new Identifier(id);
        }

        public override void Write(Utf8JsonWriter writer, Identifier identifier, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Identifier");
            writer.WritePropertyName("name");
            writer.WriteStringValue(identifier.Name.Span);
            writer.WriteEndObject();
        }

        public static bool TryGetIdentifier(JsonElement el, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out Identifier ident)
        {
            if (!el.TryGetProperty("name", out var valueElement) || valueElement.ValueKind != JsonValueKind.String)
            {
                ident = null;
                return false;
            }

            var value = valueElement.GetString() ?? "";
            ident = new Identifier(value);
            return true;
        }
    }
}
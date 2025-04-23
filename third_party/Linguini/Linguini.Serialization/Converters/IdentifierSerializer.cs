using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

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
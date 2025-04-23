using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class PatternSerializer : JsonConverter<Pattern>

    {
        public override Pattern Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var builder = new PatternBuilder();
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
                            if (typeField != "Pattern")
                            {
                                throw new JsonException(
                                    $"Invalid type: Expected 'Attribute' found {typeField} instead");
                            }

                            break;
                        case "elements":
                            AddElements(ref reader, builder, options);
                            break;
                    }
                }
            }

            return builder.Build();
        }

        private static void AddElements(ref Utf8JsonReader reader, PatternBuilder builder,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray) break;

                if (reader.TokenType != JsonTokenType.StartObject) continue;

                var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                builder.AddExpression(ReadPatternExpression(el, options));
            }
        }

        private static IPatternElement ReadPatternExpression(JsonElement el, JsonSerializerOptions options)
        {
            var type = el.GetProperty("type").GetString();
            return type switch
            {
                "TextElement" => ResourceSerializer.ProcessTextLiteral(el, options),
                "Placeable" => PlaceableSerializer.ProcessPlaceable(el, options),
                _ => throw new JsonException($"Unexpected type `{type}`")
            };
        }

        public override void Write(Utf8JsonWriter writer, Pattern pattern, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Pattern");
            writer.WritePropertyName("elements");
            writer.WriteStartArray();
            StringBuilder? textLiteralBuffer = null;
            foreach (var patternElement in pattern.Elements)
            {
                if (patternElement is TextLiteral textLiteral)
                {
                    textLiteralBuffer ??= new StringBuilder();
                    textLiteralBuffer.Append(textLiteral.Value);
                }
                else if (patternElement is Placeable placeable)
                {
                    WriteMergedText(writer, textLiteralBuffer);
                    textLiteralBuffer = null;
                    JsonSerializer.Serialize(writer, placeable, options);
                }
            }

            WriteMergedText(writer, textLiteralBuffer);

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private static void WriteMergedText(Utf8JsonWriter writer, StringBuilder? textLiteralBuffer)
        {
            if (textLiteralBuffer != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteStringValue("TextElement");
                writer.WritePropertyName("value");
                writer.WriteStringValue(textLiteralBuffer.ToString());
                writer.WriteEndObject();
            }
        }

        public static bool TryReadPattern(JsonElement jsonValue, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out Pattern pattern)
        {
            if (!jsonValue.TryGetProperty("type", out var jsonType)
                && "Placeable".Equals(jsonType.GetString()))
            {
                throw new JsonException("Placeable must have `type` equal to `Placeable`.");
            }

            if (!jsonValue.TryGetProperty("elements", out var elements)
                && elements.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("Placeable must have an `elements` array.");
            }

            var patternElements = new List<IPatternElement>();
            foreach (var element in elements.EnumerateArray())
            {
                var elementType = element.GetProperty("type").GetString();
                switch (elementType)
                {
                    case "TextElement":
                        var textValue = element.GetProperty("value").GetString() ?? "";
                        patternElements.Add(new TextLiteral(textValue));
                        break;
                    case "Placeable":
                        if (PlaceableSerializer.TryProcessPlaceable(element, options, out var placeable))
                        {
                            patternElements.Add(placeable);
                        }

                        break;
                }
            }

            pattern = new Pattern(patternElements);
            return true;
        }
    }
}
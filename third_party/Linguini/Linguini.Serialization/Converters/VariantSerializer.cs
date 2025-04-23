using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class VariantSerializer : JsonConverter<Variant>
    {
        public override Variant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadVariant(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        public override void Write(Utf8JsonWriter writer, Variant variant, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Variant");
            WriteKey(writer, variant);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, variant.Value, options);
            writer.WriteBoolean("default", variant.IsDefault);
            writer.WriteEndObject();
        }

        private static void WriteKey(Utf8JsonWriter writer, Variant value)
        {
            writer.WritePropertyName("key");
            
            writer.WriteStartObject();
            
            switch (value.Type)
            {
                case VariantType.Identifier:
                    writer.WritePropertyName("type");
                    writer.WriteStringValue("Identifier");
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(value.Key.Span);
                    break;
                case VariantType.NumberLiteral:
                    writer.WritePropertyName("value");
                    writer.WriteStringValue(value.Key.Span);
                    writer.WritePropertyName("type");
                    writer.WriteStringValue("NumberLiteral");
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Unexpected argument `{value.Type}`");
            }


            writer.WriteEndObject();
        }
        
        public static Variant ReadVariant(JsonElement el, JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("type", out var jsonType)
                && "Variant".Equals(jsonType.GetString()))
            {
                throw new JsonException("Variant must have `type` equal to `Variant`.");
            }

            if (el.TryGetProperty("key", out var jsonKey)
                && TryReadKey(jsonKey, options, out var key))
            {
                if (el.TryGetProperty("value", out var jsonValue)
                    && PatternSerializer.TryReadPattern(jsonValue, options, out var pattern))
                {
                    var isDefault = false;
                    if (el.TryGetProperty("default", out var jsonDefault))
                    {
                        isDefault = jsonDefault.ValueKind == JsonValueKind.True;
                    }

                    return new Variant(key.Value.Item1, key.Value.Item2, pattern, isDefault);
                }
            }

            throw new JsonException("Variant must have `key` and `value`.");
        }

        private static bool TryReadKey(JsonElement jsonKey, JsonSerializerOptions options,
            [NotNullWhen(true)] out (VariantType, ReadOnlyMemory<char>)? key)
        {
            if (IdentifierSerializer.TryGetIdentifier(jsonKey, options, out var id))
            {
                key = (VariantType.Identifier, id.Name);
                return true;
            }

            if (ResourceSerializer.TryReadProcessNumberLiteral(jsonKey, options, out var num))
            {
                key = (VariantType.NumberLiteral, num.Value);
                return true;
            }

            key = null;
            return false;
        }

    }
}
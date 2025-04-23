using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class NamedArgumentSerializer : JsonConverter<NamedArgument>

    {
        public override NamedArgument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (TryReadNamedArguments(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options,
                    out var namedArgument))
            {
                return namedArgument.Value;
            }

            throw new JsonException("Invalid `NamedArgument`!");
        }

        public override void Write(Utf8JsonWriter writer, NamedArgument value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("NamedArgument");
            writer.WritePropertyName("name");
            JsonSerializer.Serialize(writer, value.Name, options);
            writer.WritePropertyName("value");
            ResourceSerializer.WriteInlineExpression(writer, value.Value, options);
            writer.WriteEndObject();
        }

        public static bool TryReadNamedArguments(JsonElement el, JsonSerializerOptions options,
            [NotNullWhen(true)] out NamedArgument? o)
        {
            if (el.TryGetProperty("name", out var namedArg)
                && IdentifierSerializer.TryGetIdentifier(namedArg, options, out var id)
                && el.TryGetProperty("value", out var valueArg)
                && ResourceSerializer.TryReadInlineExpression(valueArg, options, out var inline)
               )
            {
                o = new NamedArgument(id, inline);
                return true;
            }

            throw new JsonException("NamedArgument fields `name` and `value` properties are mandatory");
        }
    }
}
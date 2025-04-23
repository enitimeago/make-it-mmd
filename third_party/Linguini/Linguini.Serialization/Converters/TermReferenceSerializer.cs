using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class TermReferenceSerializer : JsonConverter<TermReference>
    {
        public override TermReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ProcessTermReference(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        public override void Write(Utf8JsonWriter writer, TermReference value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("TermReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, value.Id, options);

            if (value.Attribute != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, value.Attribute, options);
            }

            if (value.Arguments != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("arguments");
                JsonSerializer.Serialize(writer, value.Arguments, options);
            }

            writer.WriteEndObject();
        }


        public static TermReference ProcessTermReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("id", out JsonElement value) ||
                !IdentifierSerializer.TryGetIdentifier(value, options, out var id))
            {
                throw new JsonException("Term reference must contain at least `id` field");
            }

            Identifier? attribute = null;
            CallArguments? arguments = null;
            if (el.TryGetProperty("attribute", out var attr))
            {
                IdentifierSerializer.TryGetIdentifier(attr, options, out attribute);
            }

            if (el.TryGetProperty("arguments", out var callarg))
            {
                CallArgumentsSerializer.TryGetCallArguments(callarg, options, out arguments);
            }

            return new TermReference(id, attribute, arguments);
        }
    }
}
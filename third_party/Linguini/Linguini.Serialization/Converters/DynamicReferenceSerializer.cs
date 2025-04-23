using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class DynamicReferenceSerializer : JsonConverter<DynamicReference>
    {
        public override DynamicReference? Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return ProcessDynamicReference(el, options);
        }

        public override void Write(Utf8JsonWriter writer, DynamicReference dynRef, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("DynamicReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, dynRef.Id, options);

            if (dynRef.Attribute != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, dynRef.Attribute, options);
            }

            if (dynRef.Arguments != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("arguments");
                JsonSerializer.Serialize(writer, dynRef.Arguments, options);
            }

            writer.WriteEndObject();
        }

        public static DynamicReference ProcessDynamicReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("id", out var jsonId) ||
                !IdentifierSerializer.TryGetIdentifier(jsonId, options, out var identifier))
            {
                throw new JsonException("Dynamic reference must contain at least `id` field");
            }

            Identifier? attribute = null;
            CallArguments? arguments = null;
            if (el.TryGetProperty("attribute", out var jsonAttribute))
            {
                IdentifierSerializer.TryGetIdentifier(jsonAttribute, options, out attribute);
            }

            if (el.TryGetProperty("arguments", out var jsonArgs))
            {
                CallArgumentsSerializer.TryGetCallArguments(jsonArgs, options, out arguments);
            }

            return new DynamicReference(identifier, attribute, arguments);
        }
    }
}
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class MessageReferenceSerializer : JsonConverter<MessageReference>
    {
        public override MessageReference Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return ProcessMessageReference(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        public override void Write(Utf8JsonWriter writer, MessageReference msgRef, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("MessageReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, msgRef.Id, options);
            if (msgRef.Attribute != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, msgRef.Attribute, options);
            }

            writer.WriteEndObject();
        }

        public static MessageReference ProcessMessageReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (el.TryGetProperty("id", out var getProp)
                && IdentifierSerializer.TryGetIdentifier(getProp, options, out var ident))
            {
                Identifier? attr = null;
                if (el.TryGetProperty("attribute", out var prop) && prop.ValueKind != JsonValueKind.Null)
                {
                    IdentifierSerializer.TryGetIdentifier(prop, options, out attr);
                }

                return new MessageReference(ident, attr);
            }

            throw new JsonException("MessageReference requires `id` field");
        }
    }
}
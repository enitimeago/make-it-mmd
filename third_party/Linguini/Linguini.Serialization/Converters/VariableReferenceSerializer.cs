using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class VariableReferenceSerializer : JsonConverter<VariableReference>
    {
        public override VariableReference Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return ProcessVariableReference(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        public override void Write(Utf8JsonWriter writer, VariableReference variableReference,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("VariableReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, variableReference.Id, options);
            writer.WriteEndObject();
        }

        public static VariableReference ProcessVariableReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (el.TryGetProperty("id", out var value) &&
                IdentifierSerializer.TryGetIdentifier(value, options, out var ident))
            {
                return new VariableReference(ident);
            }

            throw new JsonException("Variable reference must contain `id` field");
        }
    }
}
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class FunctionReferenceSerializer : JsonConverter<FunctionReference>
    {
        public override FunctionReference Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return ProcessFunctionReference(el, options);
        }

        public override void Write(Utf8JsonWriter writer, FunctionReference value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("FunctionReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, value.Id, options);
            writer.WritePropertyName("arguments");
            JsonSerializer.Serialize(writer, value.Arguments, options);
            writer.WriteEndObject();
        }

        public static FunctionReference ProcessFunctionReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("id", out JsonElement value) ||
                !IdentifierSerializer.TryGetIdentifier(value, options, out var ident))
            {
                throw new JsonException("Function reference must contain `id` field");
            }

            CallArguments? arguments = null;

            if (!el.TryGetProperty("arguments", out var jsonArguments) ||
                !CallArgumentsSerializer.TryGetCallArguments(jsonArguments, options, out arguments)
               )
            {
                throw new JsonException("Function reference must contain `arguments` field");
            }

            return new FunctionReference(ident, arguments.Value);
        }
    }
}
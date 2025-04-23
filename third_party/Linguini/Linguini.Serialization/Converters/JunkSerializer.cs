using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters

{
    public class JunkSerializer : JsonConverter<Junk>
    {
        public override Junk Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ProcessJunk(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        private Junk ProcessJunk(JsonElement el, JsonSerializerOptions options)
        {
            if (el.TryGetProperty("content", out var content))
            {
                var str = content.GetString() ?? "";
                return new Junk(str);
            }

            throw new JsonException("Junk must have content");
        }

        public override void Write(Utf8JsonWriter writer, Junk value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Junk");
            writer.WritePropertyName("annotations");
            writer.WriteStartArray();
            writer.WriteEndArray();
            writer.WritePropertyName("content");
            writer.WriteStringValue(value.AsStr());
            writer.WriteEndObject();
        }
    }
}

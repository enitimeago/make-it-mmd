using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Serialization.Converters
{
    public class ParseErrorSerializer : JsonConverter<ParseError>
    {
        public override ParseError? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ParseError error, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("kind");
            writer.WriteStringValue(error.Kind.ToString());
            writer.WritePropertyName("message");
            writer.WriteStringValue(error.Message);
            writer.WritePropertyName("row");
            writer.WriteNumberValue(error.Row);
            WriteRange(writer, "position", error.Position);
            WriteRange(writer, "slice", error.Slice);
            writer.WriteEndObject();
        }

        private static void WriteRange(Utf8JsonWriter writer, string name, Range? range)
        {
            if (range == null)
                return;
 
            writer.WritePropertyName(name);
            writer.WriteStartObject();
            writer.WritePropertyName("start");
            writer.WriteNumberValue(range.Value.Start.Value);
            writer.WritePropertyName("end");
            writer.WriteNumberValue(range.Value.End.Value);
            writer.WriteEndObject();
        }
    }
}
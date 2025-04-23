using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class TermSerializer : JsonConverter<AstTerm>
    {
        public override AstTerm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AstTerm term, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Term");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, term.Id, options);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, term.Value, options);

            writer.WritePropertyName("attributes");
            writer.WriteStartArray();
            foreach (var attribute in term.Attributes)
            {
                JsonSerializer.Serialize(writer, attribute, options);
            }

            writer.WriteEndArray();


            if (term.Comment != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("comment");
                JsonSerializer.Serialize(writer, term.Comment, options);
            }

            writer.WriteEndObject();
        }
    }
}

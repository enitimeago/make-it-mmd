using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class PlaceableSerializer : JsonConverter<Placeable>
    {
        public override Placeable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Placeable value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Placeable");
            writer.WritePropertyName("expression");

            switch (value.Expression)
            {
                case IInlineExpression inlineExpression:
                    ResourceSerializer.WriteInlineExpression(writer, inlineExpression, options);
                    break;
                case SelectExpression selectExpression:
                    JsonSerializer.Serialize(writer, selectExpression, options);
                    break;
            }

            writer.WriteEndObject();
        }

        public static bool TryProcessPlaceable(JsonElement el, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out Placeable placeable)
        {
            if (!el.TryGetProperty("expression", out var expr))
            {
                throw new JsonException("Placeable must have `expression` value.");
            }

            placeable = new Placeable(ResourceSerializer.ReadExpression(expr, options));
            return true;
        }

        public static Placeable ProcessPlaceable(JsonElement el, JsonSerializerOptions options)
        {
            if (!TryProcessPlaceable(el, options, out var placeable)) throw new JsonException("Expected placeable!");

            return placeable;
        }
    }
}
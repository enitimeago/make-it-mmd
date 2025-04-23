using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class SelectExpressionSerializer : JsonConverter<SelectExpression>
    {
        public override SelectExpression Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return ProcessSelectExpression(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        public override void Write(Utf8JsonWriter writer, SelectExpression value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("SelectExpression");
            writer.WritePropertyName("selector");
            ResourceSerializer.WriteInlineExpression(writer, value.Selector, options);
            writer.WritePropertyName("variants");
            writer.WriteStartArray();
            foreach (var variant in value.Variants)
            {
                JsonSerializer.Serialize(writer, variant, options);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static SelectExpression ProcessSelectExpression(JsonElement el,
            JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("selector", out var prop)) throw new JsonException("Select needs a `selector`");
            if (!ResourceSerializer.TryReadInlineExpression(prop, options, out var selector))
            {
                throw new JsonException("No inline expression found!");
            }


            if (el.TryGetProperty("variants", out var variantsProp) && variantsProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("Select `variants` must be a an array");

            var variants = new List<Variant>();
            foreach (var variantEl in variantsProp.EnumerateArray())
            {
                variants.Add(VariantSerializer.ReadVariant(variantEl, options));
            }

            return new SelectExpression(selector, variants);
        }
    }
}
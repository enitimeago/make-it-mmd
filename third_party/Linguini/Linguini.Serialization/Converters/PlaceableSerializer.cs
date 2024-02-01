// Linguini
//
// MIT License
//
// Copyright 2021 Daniel Fath
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

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
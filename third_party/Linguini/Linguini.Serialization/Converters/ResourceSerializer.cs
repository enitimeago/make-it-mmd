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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class ResourceSerializer : JsonConverter<Resource>
    {
        public override Resource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Resource value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Resource");
            writer.WritePropertyName("body");
            writer.WriteStartArray();
            foreach (var entry in value.Entries)
            {
                switch (entry)
                {
                    case AstComment astComment:
                        JsonSerializer.Serialize(writer, astComment, options);
                        break;
                    case AstMessage astMessage:
                        JsonSerializer.Serialize(writer, astMessage, options);
                        break;
                    case AstTerm astTerm:
                        JsonSerializer.Serialize(writer, astTerm, options);
                        break;
                    case Junk junk:
                        JsonSerializer.Serialize(writer, junk, options);
                        break;
                }
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void WriteInlineExpression(Utf8JsonWriter writer, IInlineExpression value,
            JsonSerializerOptions options)
        {
            if (value is TextLiteral textLiteral)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteStringValue(textLiteral.Value.Span);
                writer.WritePropertyName("type");
                writer.WriteStringValue("StringLiteral");
                writer.WriteEndObject();
            }
            else if (value is NumberLiteral numberLiteral)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteStringValue(numberLiteral.Value.Span);
                writer.WritePropertyName("type");
                writer.WriteStringValue("NumberLiteral");
                writer.WriteEndObject();
            }
            else if (value is MessageReference msgRef)
            {
                JsonSerializer.Serialize(writer, msgRef, options);
            }
            else if (value is FunctionReference funcRef)
            {
                JsonSerializer.Serialize(writer, funcRef, options);
            }
            else if (value is Placeable placeable)
            {
                JsonSerializer.Serialize(writer, placeable, options);
            }
            else if (value is TermReference termReference)
            {
                JsonSerializer.Serialize(writer, termReference, options);
            }
            else if (value is VariableReference variableReference)
            {
                JsonSerializer.Serialize(writer, variableReference, options);
            }
            else if (value is DynamicReference dynamicReference)
            {
                JsonSerializer.Serialize(writer, dynamicReference, options);
            }
        }

        public static TextLiteral ProcessTextLiteral(JsonElement el, JsonSerializerOptions options)
        {
            return new(el.GetProperty("value").GetString() ?? "");
        }

        public static NumberLiteral ProcessNumberLiteral(JsonElement el,
            JsonSerializerOptions options)
        {
            if (TryReadProcessNumberLiteral(el, options, out var numberLiteral))
            {
                return numberLiteral;
            }

            throw new JsonException("Expected value to be a valid number");
        }

        public static bool TryReadProcessNumberLiteral(JsonElement el, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out NumberLiteral numberLiteral)
        {
            if (el.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.String &&
                !"".Equals(v.GetString()))
            {
                numberLiteral = new NumberLiteral(v.GetString().AsMemory());
                return true;
            }

            numberLiteral = null;
            return false;
        }

        public static IExpression ReadExpression(JsonElement el, JsonSerializerOptions options)
        {
            var type = el.GetProperty("type").GetString();
            IExpression x = type switch
            {
                "DynamicReference" => DynamicReferenceSerializer.ProcessDynamicReference(el, options),
                "FunctionReference" => FunctionReferenceSerializer.ProcessFunctionReference(el, options),
                "MessageReference" => MessageReferenceSerializer.ProcessMessageReference(el, options),
                "NumberLiteral" => ProcessNumberLiteral(el, options),
                "Placeable" => PlaceableSerializer.ProcessPlaceable(el, options),
                "TermReference" => TermReferenceSerializer.ProcessTermReference(el, options),
                "StringLiteral" or "TextElement" or "TextLiteral" => ProcessTextLiteral(el, options),
                "VariableReference" => VariableReferenceSerializer.ProcessVariableReference(el, options),
                "SelectExpression" => SelectExpressionSerializer.ProcessSelectExpression(el, options),
                _ => throw new JsonException($"Unexpected type {type}")
            };
            return x;
        }


       
        public static bool TryReadInlineExpression(JsonElement el, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out IInlineExpression o)
        {
            var type = el.GetProperty("type").GetString();
            o = type switch
            {
                "DynamicReference" => DynamicReferenceSerializer.ProcessDynamicReference(el, options),
                "FunctionReference" => FunctionReferenceSerializer.ProcessFunctionReference(el, options),
                "MessageReference" => MessageReferenceSerializer.ProcessMessageReference(el, options),
                "NumberLiteral" => ProcessNumberLiteral(el, options),
                "Placeable" => PlaceableSerializer.ProcessPlaceable(el, options),
                "TermReference" => TermReferenceSerializer.ProcessTermReference(el, options),
                "TextLiteral" => ProcessTextLiteral(el, options),
                "VariableReference" => VariableReferenceSerializer.ProcessVariableReference(el, options),
                _ => throw new JsonException($"Unexpected value {type}")
            };
            return true;
        }
    }
}
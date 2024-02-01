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
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

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
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters

{
    public class CallArgumentsSerializer : JsonConverter<CallArguments>

    {
        public override CallArguments Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            if (TryGetCallArguments(el, options, out var value))
            {
                return value.Value;
            }

            throw new JsonException("Invalid CallArguments");
        }

        public override void Write(Utf8JsonWriter writer, CallArguments value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("CallArguments");
            writer.WritePropertyName("positional");
            writer.WriteStartArray();
            foreach (var positionalArg in value.PositionalArgs)
            {
                ResourceSerializer.WriteInlineExpression(writer, positionalArg, options);
            }

            writer.WriteEndArray();
            writer.WritePropertyName("named");
            writer.WriteStartArray();
            foreach (var namedArg in value.NamedArgs)
            {
                JsonSerializer.Serialize(writer, namedArg, options);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static bool TryGetCallArguments(JsonElement el,
            JsonSerializerOptions options,
            [NotNullWhen(true)] out CallArguments? callArguments)
        {
            if (!el.TryGetProperty("positional", out var positional) || !el.TryGetProperty("named", out var named))
            {
                throw new JsonException("CallArguments fields `positional` and `named` properties are mandatory");
            }

            var positionalArgs = new List<IInlineExpression>();
            foreach (var arg in positional.EnumerateArray())
            {
                if (ResourceSerializer.TryReadInlineExpression(arg, options, out var posArgs))
                {
                    positionalArgs.Add(posArgs);
                }
            }

            var namedArgs = new List<NamedArgument>();
            foreach (var arg in named.EnumerateArray())
            {
                if (NamedArgumentSerializer.TryReadNamedArguments(arg, options, out var namedArg))
                {
                    namedArgs.Add(namedArg.Value);
                }
            }

            callArguments = new CallArguments(positionalArgs, namedArgs);
            return true;
        }
    }
}
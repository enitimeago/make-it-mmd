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
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Parser.Error;

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
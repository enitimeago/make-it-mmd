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
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class CommentSerializer : JsonConverter<AstComment>
    {
        public override AstComment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var commentLevel = CommentLevel.None;
            var content = new List<ReadOnlyMemory<char>>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();

                    reader.Read();

                    switch (propertyName)
                    {
                        case "type":
                            var type = reader.GetString();
                            commentLevel = type switch
                            {
                                "Comment" => CommentLevel.Comment,
                                "GroupComment" => CommentLevel.GroupComment,
                                "ResourceComment" => CommentLevel.ResourceComment,
                                _ => CommentLevel.None,
                            };
                            break;
                        case "content":
                            var s = reader.GetString();
                            content = s != null 
                                ? s.Split().Select(x => x.AsMemory()).ToList() 
                                // ReSharper disable once ArrangeObjectCreationWhenTypeNotEvident
                                : new();
                            break;
                        default:
                            throw new JsonException($"Unexpected property: {propertyName}");
                    }
                }
            }

            if (commentLevel == CommentLevel.None)
            {
                throw new JsonException("Comment must have some level of nesting");
            }

            return new AstComment(commentLevel, content);
        }

        public override void Write(Utf8JsonWriter writer, AstComment comment, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            switch (comment.CommentLevel)
            {
                case CommentLevel.Comment:
                    writer.WriteStringValue("Comment");
                    break;
                case CommentLevel.GroupComment:
                    writer.WriteStringValue("GroupComment");
                    break;
                case CommentLevel.ResourceComment:
                    writer.WriteStringValue("ResourceComment");
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Unexpected comment `{comment.CommentLevel}`");
            }

            writer.WritePropertyName("content");
            writer.WriteStringValue(comment.AsStr());
            writer.WriteEndObject();
        }
    }
}
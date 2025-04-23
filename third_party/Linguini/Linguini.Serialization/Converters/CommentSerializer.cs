using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

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
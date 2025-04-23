using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Syntax.Ast
{
    public record Resource
    {
        public readonly List<IEntry> Entries;
        public readonly List<ParseError> Errors;

        public Resource(List<IEntry> body, List<ParseError> errors)
        {
            Entries = body;
            Errors = errors;
        }
    }

    public class AstMessage : IEntry, IEquatable<AstMessage>
    {
        public readonly Identifier Id;
        public readonly Pattern? Value;
        public readonly List<Attribute> Attributes;

        public AstComment? Comment => InternalComment;
        protected internal AstComment? InternalComment;

        public AstMessage(Identifier id, Pattern? pattern, List<Attribute> attrs, AstComment? internalComment)
        {
            Id = id;
            Value = pattern;
            Attributes = attrs;
            InternalComment = internalComment;
        }

        public string GetId()
        {
            return Id.ToString();
        }

        public bool Equals(AstMessage? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Comparator.Equals(Id, other.Id) && Equals(Value, other.Value) &&
                   Attributes.SequenceEqual(other.Attributes, Attribute.Comparer) &&
                   Equals(InternalComment, other.InternalComment);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AstMessage)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value, Attributes, InternalComment);
        }
    }

    public class AstTerm : IEntry
    {
        public readonly Identifier Id;
        public readonly Pattern Value;
        public readonly List<Attribute> Attributes;
        public AstComment? Comment => InternalComment;
        protected internal AstComment? InternalComment;

        public AstTerm(Identifier id, Pattern value, List<Attribute> attributes, AstComment? comment)
        {
            Id = id;
            Value = value;
            Attributes = attributes;
            InternalComment = comment;
        }


        public string GetId()
        {
            return Id.ToString();
        }
    }

    public class AstComment : IEntry, IEquatable<AstComment>
    {
        public readonly CommentLevel CommentLevel;
        public readonly List<ReadOnlyMemory<char>> Content;

        public AstComment(CommentLevel commentLevel, List<ReadOnlyMemory<char>> content)
        {
            CommentLevel = commentLevel;
            Content = content;
        }

        public string AsStr(string lineEnd = "\n")
        {
            StringBuilder sb = new();
            for (int i = 0; i < Content.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(lineEnd);
                }

                sb.Append(Content[i].Span.ToString());
            }

            return sb.ToString();
        }

        public string GetId()
        {
            return "Comment";
        }

        public bool Equals(AstComment? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (CommentLevel != other.CommentLevel) return false;
            if (Content.Count != other.Content.Count) return false;
            for (int i = 0; i < Content.Count; i++)
            {
                var l = Content[i];
                var r = other.Content[i];
                if (!l.Span.SequenceEqual(r.Span))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AstComment)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)CommentLevel, Content);
        }
    }

    public class Junk : IEntry, IEquatable<Junk>
    {
        public readonly ReadOnlyMemory<char> Content;

        public Junk()
        {
            Content = ReadOnlyMemory<char>.Empty;
        }

        public Junk(ReadOnlyMemory<char> content)
        {
            Content = content;
        }

        public Junk(string content)
        {
            Content = content.AsMemory();
        }

        public string AsStr()
        {
            return Content.Span.ToString();
        }

        public string GetId()
        {
            return Content.Span.ToString();
        }

        public bool Equals(Junk? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Content.Span.SequenceEqual(other.Content.Span);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Junk)obj);
        }

        public override int GetHashCode()
        {
            return Content.GetHashCode();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ForCanBeConvertedToForeach

namespace Linguini.Syntax.Ast
{
    public class Attribute : IEquatable<Attribute>
    {
        public readonly Identifier Id;
        public readonly Pattern Value;

        public static AttributeComparer Comparer = new();

        public class AttributeComparer : IEqualityComparer<Attribute>
        {
            public bool Equals(Attribute? x, Attribute? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return Identifier.Comparator.Equals(x.Id, y.Id) &&
                       x.Value.Equals(y.Value);
            }

            public int GetHashCode(Attribute obj)
            {
                return HashCode.Combine(obj.Id, obj.Value);
            }
        }

        public Attribute(Identifier id, Pattern value)
        {
            Id = id;
            Value = value;
        }

        public Attribute(string id, PatternBuilder builder)
        {
            Id = new Identifier(id);
            Value = builder.Build();
        }

        public void Deconstruct(out Identifier id, out Pattern value)
        {
            id = Id;
            value = Value;
        }

        public static Attribute From(string id, PatternBuilder patternBuilder)
        {
            return new Attribute(new Identifier(id), patternBuilder.Build());
        }

        public bool Equals(Attribute? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Attribute)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value);
        }
    }

    public class Pattern : IEquatable<Pattern>
    {
        public readonly List<IPatternElement> Elements;


        public Pattern(List<IPatternElement> elements)
        {
            Elements = elements;
        }

        public Pattern()
        {
            Elements = new List<IPatternElement>();
        }

        public bool Equals(Pattern? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Elements.Count != other.Elements.Count)
            {
                return false;
            }

            for (var index = 0; index < Elements.Count; index++)
            {
                var patternElement = Elements[index];
                var otherPatternElement = other.Elements[index];
                if (!IPatternElement.PatternComparer.Equals(patternElement, otherPatternElement))
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
            return Equals((Pattern)obj);
        }

        public override int GetHashCode()
        {
            return Elements.GetHashCode();
        }
    }

    public class PatternBuilder
    {
        private readonly List<IPatternElement> _patternElements = new();

        public PatternBuilder()
        {
        }

        public PatternBuilder(string text)
        {
            _patternElements.Add(new TextLiteral(text));
        }

        public PatternBuilder(float number)
        {
            _patternElements.Add(new Placeable(new NumberLiteral(number)));
        }

        public PatternBuilder AddText(string textLiteral)
        {
            _patternElements.Add(new TextLiteral(textLiteral));
            return this;
        }

        public PatternBuilder AddNumberLiteral(float number)
        {
            _patternElements.Add(new Placeable(new NumberLiteral(number)));
            return this;
        }

        public PatternBuilder AddNumberLiteral(double number)
        {
            _patternElements.Add(new Placeable(new NumberLiteral(number)));
            return this;
        }

        public PatternBuilder AddMessage(string id, string? attribute = null)
        {
            _patternElements.Add(new Placeable(new MessageReference(id, attribute)));
            return this;
        }

        public PatternBuilder AddTermReference(string id, string? attribute = null, CallArguments? callArguments = null)
        {
            _patternElements.Add(new Placeable(new TermReference(id, attribute, callArguments)));
            return this;
        }

        public PatternBuilder AddDynamicReference(string id, string? attribute = null,
            CallArguments? callArguments = null)
        {
            _patternElements.Add(new Placeable(new DynamicReference(id, attribute, callArguments)));
            return this;
        }

        public PatternBuilder AddFunctionReference(string functionName, CallArguments funcArgs = default)
        {
            _patternElements.Add(new Placeable(new FunctionReference(functionName, funcArgs)));
            return this;
        }

        public PatternBuilder AddFunctionReference(string functionName, CallArgumentsBuilder builder)
        {
            _patternElements.Add(new Placeable(new FunctionReference(functionName, builder.Build())));
            return this;
        }

        public PatternBuilder AddMessageReference(string messageId, string? attribute = null)
        {
            _patternElements.Add(new Placeable(new MessageReference(messageId, attribute)));
            return this;
        }

        public PatternBuilder AddSelectExpression(SelectExpressionBuilder selectExpressionBuilder)
        {
            _patternElements.Add(new Placeable(selectExpressionBuilder.Build()));
            return this;
        }

        public PatternBuilder AddExpression(IPatternElement expr)
        {
            _patternElements.Add(expr);
            return this;
        }

        public PatternBuilder AddPlaceable(Placeable placeable)
        {
            _patternElements.Add(placeable);
            return this;
        }

        public Pattern Build()
        {
            return new Pattern(_patternElements);
        }
    }


    public class Identifier : IEquatable<Identifier>
    {
        public class IdentifierComparator : IEqualityComparer<Identifier>
        {
            public bool Equals(Identifier? x, Identifier? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Name.Span.SequenceEqual(y.Name.Span);
            }

            public int GetHashCode(Identifier obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        public readonly ReadOnlyMemory<char> Name;

        public static readonly IdentifierComparator Comparator = new();

        public Identifier(ReadOnlyMemory<char> name)
        {
            Name = name;
        }

        public Identifier(string id)
        {
            Name = id.AsMemory();
        }

        public override string ToString()
        {
            return Name.Span.ToString();
        }

        public bool Equals(Identifier? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Comparator.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return Comparator.GetHashCode(this);
        }
    }

    public interface IExpression
    {
    }

    public enum CommentLevel : byte
    {
        None = 0,
        Comment = 1,
        GroupComment = 2,
        ResourceComment = 3,
    }

    public interface IEntry
    {
        string GetId();
    }

    public interface IInlineExpression : IExpression
    {
        public static readonly InlineExpressionComparer Comparer = new();
    }

    public class InlineExpressionComparer : IEqualityComparer<IInlineExpression>
    {
        public bool Equals(IInlineExpression? left, IInlineExpression? right)
        {
            return (left, right) switch
            {
                (DynamicReference l, DynamicReference r) => l.Equals(r),
                (FunctionReference l, FunctionReference r) => l.Equals(r),
                (MessageReference l, MessageReference r) => l.Equals(r),
                (NumberLiteral l, NumberLiteral r) => l.Equals(r),
                (Placeable l, Placeable r) => l.Equals(r),
                (TermReference l, TermReference r) => l.Equals(r),
                (TextLiteral l, TextLiteral r) => l.Equals(r),
                (VariableReference l, VariableReference r) => l.Equals(r),
                _ => false
            };
        }

        public int GetHashCode(IInlineExpression obj)
        {
            return obj switch
            {
                DynamicReference dr => dr.GetHashCode(),
                FunctionReference fr => fr.GetHashCode(),
                MessageReference mr => mr.GetHashCode(),
                NumberLiteral nl => nl.GetHashCode(),
                Placeable p => p.GetHashCode(),
                TermReference term => term.GetHashCode(),
                TextLiteral tl => tl.GetHashCode(),
                VariableReference vr => vr.GetHashCode(),
                _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
            };
        }
    }

    public static class Base
    {
        public static string Stringify(this Pattern? pattern)
        {
            var sb = new StringBuilder();
            if (pattern == null || pattern.Elements.Count <= 0) return sb.ToString();
            for (var i = 0; i < pattern.Elements.Count; i++)
            {
                sb.Append(pattern.Elements[i]);
            }

            return sb.ToString();
        }
    }
}
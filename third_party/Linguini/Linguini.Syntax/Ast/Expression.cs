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
using System.Globalization;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast
{
    public class TextLiteral : IInlineExpression, IPatternElement, IEquatable<TextLiteral>
    {
        public readonly ReadOnlyMemory<char> Value;

        public TextLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }

        public TextLiteral(string id)
        {
            Value = id.AsMemory();
        }

        public override string ToString()
        {
            return Value.Span.ToString();
        }

        public bool Equals(TextLiteral? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Span.SequenceEqual(other.Value.Span);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextLiteral)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class NumberLiteral : IInlineExpression, IEquatable<NumberLiteral>
    {
        public readonly ReadOnlyMemory<char> Value;

        public NumberLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }

        public NumberLiteral(float num)
        {
            Value = num.ToString(CultureInfo.InvariantCulture).AsMemory();
        }

        public NumberLiteral(double num)
        {
            Value = num.ToString(CultureInfo.InvariantCulture).AsMemory();
        }

        public override string ToString()
        {
            return Value.Span.ToString();
        }

        public bool Equals(NumberLiteral? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Span.SequenceEqual(other.Value.Span);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NumberLiteral)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }


    public class FunctionReference : IInlineExpression, IEquatable<FunctionReference>
    {
        public readonly Identifier Id;
        public readonly CallArguments Arguments;

        public FunctionReference(Identifier id, CallArguments arguments)
        {
            Id = id;
            Arguments = arguments;
        }

        public FunctionReference(string id, CallArguments arguments)
        {
            Id = new Identifier(id);
            Arguments = arguments;
        }

        public bool Equals(FunctionReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Arguments.Equals(other.Arguments);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FunctionReference)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Arguments);
        }
    }

    public class MessageReference : IInlineExpression, IEquatable<MessageReference>
    {
        public readonly Identifier Id;
        public readonly Identifier? Attribute;

        public MessageReference(Identifier id, Identifier? attribute)
        {
            Id = id;
            Attribute = attribute;
        }

        public MessageReference(string id, string? attribute = null)
        {
            Id = new Identifier(id);
            if (attribute != null)
            {
                Attribute = new Identifier(attribute);
            }
        }

        public bool Equals(MessageReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Identifier.Comparator.Equals(Attribute, other.Attribute);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MessageReference)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Attribute);
        }
    }

    public class DynamicReference : IInlineExpression, IEquatable<DynamicReference>
    {
        public readonly Identifier Id;
        public readonly Identifier? Attribute;
        public readonly CallArguments? Arguments;

        public DynamicReference(Identifier id, Identifier? attribute, CallArguments? arguments)
        {
            Id = id;
            Attribute = attribute;
            Arguments = arguments;
        }

        public DynamicReference(string id, string? attribute = null, CallArguments? arguments = null)
        {
            Id = new Identifier(id);
            if (attribute != null)
            {
                Attribute = new Identifier(attribute);
            }

            if (arguments != null)
            {
                Arguments = arguments.Value;
            }
        }

        public DynamicReference(string id, string? attribute, CallArgumentsBuilder? callArgumentsBuilder)
        {
            Id = new Identifier(id);
            if (attribute != null)
            {
                Attribute = new Identifier(attribute);
            }

            if (callArgumentsBuilder != null)
            {
                Arguments = callArgumentsBuilder.Build();
            }
        }

        public bool Equals(DynamicReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Identifier.Comparator.Equals(Attribute, other.Attribute) &&
                   Nullable.Equals(Arguments, other.Arguments);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DynamicReference)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Attribute, Arguments);
        }
    }

    public class TermReference : IInlineExpression, IEquatable<TermReference>
    {
        public readonly Identifier Id;
        public readonly Identifier? Attribute;
        public readonly CallArguments? Arguments;

        public TermReference(Identifier id, Identifier? attribute, CallArguments? arguments)
        {
            Id = id;
            Attribute = attribute;
            Arguments = arguments;
        }

        public TermReference(string id, string? attribute = null, CallArguments? arguments = null)
        {
            Id = new Identifier(id);
            if (attribute != null)
            {
                Attribute = new Identifier(attribute);
            }

            if (arguments != null)
            {
                Arguments = arguments.Value;
            }
        }

        public TermReference(string id, string? attribute, CallArgumentsBuilder? argumentsBuilder)
        {
            Id = new Identifier(id);
            if (attribute != null)
            {
                Attribute = new Identifier(attribute);
            }

            if (argumentsBuilder != null)
            {
                Arguments = argumentsBuilder.Build();
            }
        }

        public bool Equals(TermReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Identifier.Comparator.Equals(Attribute, other.Attribute) &&
                   Nullable.Equals(Arguments, other.Arguments);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TermReference)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Attribute, Arguments);
        }
    }

    public class VariableReference : IInlineExpression, IEquatable<VariableReference>
    {
        public readonly Identifier Id;

        public VariableReference(Identifier id)
        {
            Id = id;
        }

        public VariableReference(string id)
        {
            Id = new Identifier(id);
        }

        public bool Equals(VariableReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Comparator.Equals(Id , other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VariableReference)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class Placeable : IInlineExpression, IPatternElementPlaceholder, IPatternElement, IEquatable<Placeable>
    {
        public readonly IExpression Expression;

        public Placeable(IExpression expression)
        {
            Expression = expression;
        }

        public bool Equals(Placeable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Expression.Equals(other.Expression);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Placeable)obj);
        }

        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }
    }

    public class PlaceableBuilder
    {
        private readonly IExpression _expression;

        private PlaceableBuilder(IExpression expression)
        {
            _expression = expression;
        }

        public static PlaceableBuilder InlineExpression(InlineExpressionBuilder inlineBuilder)
        {
            return new PlaceableBuilder(inlineBuilder.Build());
        }

        public static PlaceableBuilder InlineExpression(SelectExpressionBuilder selectorExpression)
        {
            return new PlaceableBuilder(selectorExpression.Build());
        }

        public Placeable Build()
        {
            return new Placeable(_expression);
        }
    }

    public struct CallArguments : IEquatable<CallArguments>
    {
        public readonly List<IInlineExpression> PositionalArgs;
        public readonly List<NamedArgument> NamedArgs;

        public CallArguments(List<IInlineExpression> positionalArgs, List<NamedArgument> namedArgs)
        {
            PositionalArgs = positionalArgs;
            NamedArgs = namedArgs;
        }

        public bool Equals(CallArguments other)
        {
            return PositionalArgs.SequenceEqual(other.PositionalArgs, IInlineExpression.Comparer)
                   && NamedArgs.SequenceEqual(other.NamedArgs);
        }

        public override bool Equals(object? obj)
        {
            return obj is CallArguments other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PositionalArgs, NamedArgs);
        }
    }

    public readonly struct NamedArgument : IEquatable<NamedArgument>
    {
        public readonly Identifier Name;
        public readonly IInlineExpression Value;

        public NamedArgument(Identifier name, IInlineExpression value)
        {
            Name = name;
            Value = value;
        }

        public bool Equals(NamedArgument other)
        {
            return Name.Equals(other.Name) && IInlineExpression.Comparer.Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is NamedArgument other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Value);
        }
    }

    public class CallArgumentsBuilder
    {
        private readonly List<IInlineExpression> _positionalArgs = new();
        private readonly List<NamedArgument> _namedArgs = new();

        public CallArgumentsBuilder AddPositionalArg(InlineExpressionBuilder arg)
        {
            _positionalArgs.Add(arg.Build());
            return this;
        }

        public CallArgumentsBuilder AddPositionalArg(string text)
        {
            _positionalArgs.Add(new TextLiteral(text));
            return this;
        }

        public CallArgumentsBuilder AddPositionalArg(double number)
        {
            _positionalArgs.Add(new NumberLiteral(number));
            return this;
        }

        public CallArgumentsBuilder AddPositionalArg(float number)
        {
            _positionalArgs.Add(new NumberLiteral(number));
            return this;
        }

        public CallArgumentsBuilder AddNamedArg(string identifier, InlineExpressionBuilder inlineExpression)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), inlineExpression.Build()));
            return this;
        }

        public CallArgumentsBuilder AddNamedArg(string identifier, IInlineExpression inlineExpression)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), inlineExpression));
            return this;
        }

        public CallArgumentsBuilder AddNamedArg(string identifier, string text)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), new TextLiteral(text)));
            return this;
        }

        public CallArgumentsBuilder AddNamedArg(string identifier, float number)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), new NumberLiteral(number)));
            return this;
        }

        public CallArgumentsBuilder AddNamedArg(string identifier, double number)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), new NumberLiteral(number)));
            return this;
        }

        public CallArguments Build()
        {
            return new CallArguments(_positionalArgs, _namedArgs);
        }
    }

    public class InlineExpressionBuilder
    {
        private IInlineExpression _expression;

        private InlineExpressionBuilder(IInlineExpression expression)
        {
            _expression = expression;
        }

        public static InlineExpressionBuilder CreateDynamicReference(string id, string? attribute = null,
            CallArgumentsBuilder? callArgumentsBuilder = null)
        {
            return new InlineExpressionBuilder(new DynamicReference(id, attribute, callArgumentsBuilder));
        }

        public static InlineExpressionBuilder CreateFunctionReference(string id,
            CallArgumentsBuilder callArgumentsBuilder)
        {
            return new InlineExpressionBuilder(new FunctionReference(id, callArgumentsBuilder.Build()));
        }

        public static InlineExpressionBuilder CreateMessageReference(string id, string? attribute = null)
        {
            return new InlineExpressionBuilder(new MessageReference(id, attribute));
        }

        public static InlineExpressionBuilder CreateNumber(double numberLiteral)
        {
            return new InlineExpressionBuilder(new NumberLiteral(numberLiteral));
        }

        public static InlineExpressionBuilder CreateNumber(float numberLiteral)
        {
            return new InlineExpressionBuilder(new NumberLiteral(numberLiteral));
        }

        public static InlineExpressionBuilder CreatePlaceable(Placeable placeable)
        {
            return new InlineExpressionBuilder(placeable);
        }

        public static InlineExpressionBuilder CreatePlaceable(PlaceableBuilder placeable)
        {
            return new InlineExpressionBuilder(placeable.Build());
        }

        public static InlineExpressionBuilder CreateTermReference(string id, string? attribute = null,
            CallArgumentsBuilder? callArgumentsBuilder = null)
        {
            return new InlineExpressionBuilder(new TermReference(id, attribute, callArgumentsBuilder));
        }

        public static InlineExpressionBuilder CreateTextLiteral(string textLiteral)
        {
            return new InlineExpressionBuilder(new TextLiteral(textLiteral));
        }

        public static InlineExpressionBuilder CreateVariableReferences(string textLiteral)
        {
            return new InlineExpressionBuilder(new VariableReference(textLiteral));
        }

        public IInlineExpression Build()
        {
            return _expression;
        }
    }

    public class SelectExpression : IExpression, IEquatable<SelectExpression>
    {
        public readonly IInlineExpression Selector;
        public readonly List<Variant> Variants;

        public SelectExpression(IInlineExpression selector, List<Variant> variants)
        {
            Selector = selector;
            Variants = variants;
        }

        public bool Equals(SelectExpression? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IInlineExpression.Comparer.Equals(Selector, other.Selector)
                   && Variants.SequenceEqual(other.Variants);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SelectExpression)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Selector, Variants);
        }
    }

    public class SelectExpressionBuilder : IAddVariant
    {
        private readonly IInlineExpression _selector;
        private readonly List<Variant> _variants = new();

        public SelectExpressionBuilder(IInlineExpression selector)
        {
            _selector = selector;
        }

        public IAddVariant AddVariant(string selector, PatternBuilder patternBuilder)
        {
            _variants.Add(new Variant(selector, patternBuilder));
            return this;
        }

        public IAddVariant AddVariant(float selector, PatternBuilder patternBuilder)
        {
            _variants.Add(new Variant(selector, patternBuilder));
            return this;
        }

        public SelectExpressionBuilder SetDefault(int? defaultSelector = null)
        {
            var selector = defaultSelector is >= 0 && defaultSelector < _variants.Count
                ? _variants.Count - 1
                : defaultSelector!.Value;
            _variants[selector].InternalDefault = true;
            return this;
        }

        public SelectExpression Build()
        {
            return new SelectExpression(_selector, _variants);
        }
    }

    public interface IAddVariant
    {
        public IAddVariant AddVariant(string selector, PatternBuilder patternBuilder);
        public IAddVariant AddVariant(float selector, PatternBuilder patternBuilder);
        public SelectExpressionBuilder SetDefault(int? defaultSelector = null);
    }

    public enum VariantType : byte
    {
        Identifier,
        NumberLiteral,
    }


    public class Variant : IEquatable<Variant>
    {
        public readonly VariantType Type;
        public readonly ReadOnlyMemory<char> Key;
        public Pattern Value => InternalValue;
        public bool IsDefault => InternalDefault;

        protected internal bool InternalDefault;
        protected internal Pattern InternalValue;

        public Variant(VariantType type, ReadOnlyMemory<char> key)
        {
            Type = type;
            Key = key;
            InternalValue = new Pattern();
            InternalDefault = false;
        }

        public Variant(VariantType type, ReadOnlyMemory<char> key, Pattern pattern, bool isDefault = false)
        {
            Type = type;
            Key = key;
            InternalValue = pattern;
            InternalDefault = isDefault;
        }

        public Variant(string key, PatternBuilder builder)
        {
            Type = VariantType.Identifier;
            Key = key.AsMemory();
            InternalValue = builder.Build();
            InternalDefault = false;
        }

        public Variant(float key, PatternBuilder builder)
        {
            Type = VariantType.NumberLiteral;
            Key = key.ToString(CultureInfo.InvariantCulture).AsMemory();
            InternalValue = builder.Build();
            InternalDefault = false;
        }

        public bool Equals(Variant? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && Key.Span.SequenceEqual(other.Key.Span) &&
                   InternalDefault == other.InternalDefault &&
                   InternalValue.Equals(other.InternalValue);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Variant)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, Key, InternalDefault, InternalValue);
        }
    }
}
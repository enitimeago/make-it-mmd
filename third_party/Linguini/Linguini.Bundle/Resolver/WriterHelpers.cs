using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types.Bundle;
using Linguini.Shared.Util;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Resolver
{
    public static class WriterHelpers
    {
        public static void Write(this Pattern pattern, TextWriter writer, Scope scope)
        {
            var len = pattern.Elements.Count;
            var transformFunc = scope.TransformFunc;
            var placeablePos = 0;
            for (var i = 0; i < len; i++)
            {
                if (scope.Dirty)
                {
                    return;
                }

                var elem = pattern.Elements[i];

                if (elem is TextLiteral textLiteral)
                {
                    if (transformFunc != null)
                    {
                        writer.Write(transformFunc(textLiteral.ToString()));
                    }
                    else
                    {
                        writer.Write(textLiteral.Value.Span.ToString());
                    }
                }
                else if (elem is Placeable placeable)
                {
                    var expr = placeable.Expression;
                    if (!scope.IncrPlaceable())
                    {
                        scope.Dirty = true;
                        scope.AddError(ResolverFluentError.TooManyPlaceables());
                        return;
                    }

                    var needsIsolating = scope.UseIsolating
                                         && len > 1;

                    if (needsIsolating)
                    {
                        writer.Write('\u2068');
                    }

                    scope.MaybeTrack(writer, pattern, expr, placeablePos++);

                    if (needsIsolating)
                    {
                        writer.Write('\u2069');
                    }
                }
            }
        }

        public static bool TryWrite(this IExpression expression, TextWriter writer, Scope scope, int? pos = null)
        {
            var errors = new List<FluentError>();
            if (expression is IInlineExpression inlineExpression)
            {
                inlineExpression.Write(writer, scope);
            }
            else if (expression is SelectExpression selectExpression)
            {
                var selector = selectExpression.Selector.Resolve(scope, pos);
                if (selector is FluentString or FluentNumber)
                {
                    foreach (var variant in selectExpression.Variants)
                    {
                        IFluentType key;
                        switch (variant.Type)
                        {
                            case VariantType.NumberLiteral:
                                key = FluentNumber.TryNumber(variant.Key.Span);
                                break;
                            default:
                                key = new FluentString(variant.Key.Span);
                                break;
                        }

                        if (key.Matches(selector, scope))
                        {
                            variant.Value.Write(writer, scope);
                            return scope.Errors.Count == 0;
                        }
                    }
                }

                for (var i = 0; i < selectExpression.Variants.Count; i++)
                {
                    var variant = selectExpression.Variants[i];
                    if (!variant.IsDefault) continue;
                    variant.Value.Write(writer, scope);
                    return errors.Count == 0;
                }

                errors.Add(ResolverFluentError.MissingDefault());
            }

            return scope.Errors.Count == 0;
        }

        private static void Write(this IInlineExpression self, TextWriter writer, Scope scope)
        {
            if (self is TextLiteral textLiteral)
            {
                UnicodeUtil.WriteUnescapedUnicode(textLiteral.Value, writer);
                return;
            }

            if (self is NumberLiteral numberLiteral)
            {
                var value = FluentNumber.TryNumber(numberLiteral.Value.Span);
                value.Write(writer, scope);
                return;
            }

            if (self is MessageReference msgRef)
            {
                ProcessMsgRef(self, writer, scope, msgRef);
                return;
            }

            if (self is TermReference termRef)
            {
                var res = scope.GetArguments(termRef.Arguments);
                scope.SetLocalArgs(res.Named);
                if (scope.Bundle.TryGetAstTerm(termRef.Id.ToString(), out var term))
                {
                    var attrName = termRef.Attribute;
                    var attr = term
                        .Attributes
                        .Find(a => a.Id.Equals(attrName));

                    scope.Track(writer, attr != null ? attr.Value : term.Value, self);
                }
                else
                {
                    scope.WriteRefError(writer, self);
                }

                scope.ClearLocalArgs();
                return;
            }

            if (self is DynamicReference dynRef)
            {
                ProcessDynRef(self, writer, scope, dynRef);
                return;
            }

            if (self is FunctionReference funcRef)
            {
                var (resolvedPosArgs, resolvedNamedArgs) = scope.GetArguments(funcRef.Arguments);

                if (scope.Bundle.TryGetFunction(funcRef.Id, out var func))
                {
                    var result = func.Function(resolvedPosArgs, resolvedNamedArgs);
                    if (result.IsError())
                    {
                        self.WriteError(writer);
                    }
                    else
                    {
                        writer.Write(result.AsString());
                    }
                }
                else
                {
                    scope.WriteRefError(writer, self);
                }
            }

            if (self is VariableReference varRef)
            {
                var id = varRef.Id;
                var args = scope.LocalNameArgs ?? scope.Args;

                if (args != null
                    && args.TryGetValue(id.ToString(), out var arg))
                {
                    arg.Write(writer, scope);
                }
                else
                {
                    if (scope.LocalNameArgs == null)
                    {
                        scope.AddError(ResolverFluentError.Reference(self));
                    }

                    writer.Write('{');
                    self.WriteError(writer);
                    writer.Write('}');
                }
            }

            if (self is Placeable placeable)
            {
                placeable.Expression.TryWrite(writer, scope);
            }
        }

        private static void Write(this IFluentType self, TextWriter writer, Scope scope)
        {
            if (scope.FormatterFunc != null)
            {
                writer.Write(scope.FormatterFunc(self));
            }

            writer.Write(self.AsString());
        }

        private static void ProcessMsgRef(IInlineExpression self, TextWriter writer, Scope scope,
            MessageReference msgRef)
        {
            var id = msgRef.Id;
            var attribute = msgRef.Attribute;

            if (scope.Bundle.TryGetAstMessage(id.Name.ToString(), out var msg))
            {
                if (attribute != null)
                {
                    var found = msg.Attributes.Find(e => e.Id.Equals(attribute));
                    if (found != null)
                    {
                        scope.Track(writer, found.Value, self);
                    }
                    else
                    {
                        scope.WriteRefError(writer, self);
                    }
                }
                else
                {
                    if (msg.Value != null)
                    {
                        scope.Track(writer, msg.Value, self);
                    }
                    else
                    {
                        scope.AddError(ResolverFluentError.NoValue(id.Name));
                        writer.Write('{');
                        self.WriteError(writer);
                        writer.Write('}');
                    }
                }
            }
            else
            {
                scope.WriteRefError(writer, self);
            }
        }

        private static void ProcessDynRef(IInlineExpression self, TextWriter writer, Scope scope,
            DynamicReference dynRef)
        {
            var res = scope.GetArguments(dynRef.Arguments);
            scope.SetLocalArgs(res);
            var strRef = scope.ResolveReference(dynRef.Id);
            
            
            if (scope.Bundle.TryGetAstTerm(strRef, out var term))
            {
                var attrName = dynRef.Attribute;
                var attr = term
                    .Attributes
                    .Find(a => a.Id.Equals(attrName));

                scope.Track(writer, attr != null ? attr.Value : term.Value, self);
            }
            else if (scope.Bundle.TryGetAstMessage(strRef, out var astMessage))
            {
                var attrName = dynRef.Attribute;
                var attr = astMessage
                    .Attributes
                    .Find(a => a.Id.Equals(attrName));

                scope.Track(writer, attr != null ? attr.Value : astMessage.Value!, self);
            }
            else
            {
                scope.WriteRefError(writer, self);
            }

            scope.ClearLocalArgs();
        }

        public static void WriteError(this IExpression self, TextWriter writer)
        {
            if (self is IInlineExpression expr)
            {
                expr.WriteError(writer);
            }
            else if (self is SelectExpression)
            {
                throw new ArgumentException("Unexpected select expression!");
            }
        }

        public static void WriteError(this IInlineExpression self, TextWriter writer)
        {
            switch (self)
            {
                case MessageReference msgRef:
                    writer.Write(msgRef.Attribute == null ? $"{msgRef.Id}" : $"{msgRef.Id}.{msgRef.Attribute}");
                    return;
                case TermReference termReference:
                    writer.Write(termReference.Attribute == null ? $"-{termReference.Id}" : $"-{termReference.Id}.{termReference.Attribute}");
                    return;
                case FunctionReference funcRef:
                    writer.Write($"{funcRef.Id}()");
                    return;
                case VariableReference varRef:
                    writer.Write($"${varRef.Id}");
                    return;
                case DynamicReference dynamicReference:
                    writer.Write($"$${dynamicReference.Id}");
                    return;
                default:
                    throw new ArgumentException($"Unexpected inline expression `{self.GetType()}`!");
            }
        }
    }
}
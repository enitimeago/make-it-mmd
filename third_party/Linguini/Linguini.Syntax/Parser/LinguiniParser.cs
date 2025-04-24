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
using System.IO;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Util;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.IO;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Parser.Error;
using Attribute = enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast.Attribute;

#pragma warning disable 8600

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Parser
{
    /// <summary>
    /// Zero copy parser for Fluent system.
    /// </summary>
    public class LinguiniParser
    {
        private readonly ZeroCopyReader _reader;
        private readonly bool _enableExperimental;
        private const string Cr = "\n";

        /// <summary>
        /// Set input to <c>string</c>
        /// </summary>
        /// <param name="input">Input to be parsed</param>
        /// <param name="enableExperimental">Using non-standard Fluent extensions</param>
        public LinguiniParser(string input, bool enableExperimental = false)
        {
            _reader = new ZeroCopyReader(input);
            _enableExperimental = enableExperimental;
        }

        /// <summary>
        /// Set input to <c>TextReader</c>
        /// </summary>
        /// <param name="input">TextReader to be parsed to Fluent AST.</param>
        /// <param name="enableExperimental">Using non-standard Fluent extensions</param>
        public LinguiniParser(TextReader input, bool enableExperimental = false)
        {
            using (input)
            {
                _reader = new ZeroCopyReader(input.ReadToEnd());
                _enableExperimental = enableExperimental;
            }
        }

        public ReadOnlyMemory<char> GetReadonlyData => _reader.GetData;

        #region FastParse

        /// <summary>
        /// Convert the previously set input to Fluent AST. 
        /// </summary>
        /// <returns>Fluent AST resource.</returns>
        public Resource Parse()
        {
            var body = new List<IEntry>(6);
            var errors = new List<ParseError>();

            _reader.SkipBlankBlock();

            while (_reader.IsNotEof)
            {
                var entryStart = _reader.Position;
                (var entry, ParseError? error) = GetEntryRuntime(entryStart);
                if (entry is { } and not Junk)
                {
                    body.Add(entry);
                }

                if (error != null)
                {
                    AddError(error, entryStart, errors, body);
                }

                _reader.SkipBlankBlock();
            }

            return new Resource(body, errors);
        }

        private void SkipComment()
        {
            while (_reader.SeekEol())
            {
                if (_reader.TryPeekChar(out var c) && c == '#')
                {
                    _reader.Position += 1;
                }
                else
                {
                    return;
                }
            }
        }

        private (IEntry?, ParseError?) GetEntryRuntime(int entryStart)
        {
            if (_reader.TryPeekChar(out var c))
            {
                if (c == '#')
                {
                    SkipComment();
                    return (null, null);
                }

                if (c == '-')
                {
                    return GetTerm(entryStart);
                }
            }

            return GetMessage(entryStart);
        }

        #endregion


        /// <summary>
        /// Convert the previously set input to Fluent AST, ignoring comments.
        /// </summary>
        /// <returns>Fluent AST resource.</returns>
        public Resource ParseWithComments()
        {
            var body = new List<IEntry>();
            var errors = new List<ParseError>();
            _reader.SkipBlankBlock();

            AstComment? lastComment = null;
            var lastBlankCount = 0;

            while (_reader.IsNotEof)
            {
                var entryStart = _reader.Position;
                (IEntry entry, ParseError? error) = GetEntry(entryStart);

                if (lastComment != null)
                {
                    if (entry is AstMessage message
                        && lastBlankCount < 2)
                    {
                        message.InternalComment = lastComment;
                    }
                    else if (entry is AstTerm term
                             && lastBlankCount < 2)
                    {
                        term.InternalComment = lastComment;
                    }
                    else
                    {
                        body.Add(lastComment);
                    }

                    lastComment = null;
                }

                if (error != null)
                {
                    AddError(error, entryStart, errors, body);
                }
                else if (entry is AstComment { CommentLevel: CommentLevel.Comment } comment)
                {
                    lastComment = comment;
                }
                else
                {
                    body.Add(entry);
                }

                lastBlankCount = _reader.SkipBlankBlock();
            }

            if (lastComment != null)
            {
                body.Add(lastComment);
            }


            return new Resource(body, errors);
        }

        private void AddError(ParseError error, int entryStart, List<ParseError> errors, List<IEntry> body)
        {
            _reader.SkipToNextEntry();
            error.Slice = new Range(entryStart, _reader.Position);
            errors.Add(error);
           
            var contentSpan = _reader.ReadSlice(entryStart, _reader.Position);
            Junk junk = new(contentSpan);
            body.Add(junk);
        }

        private (IEntry, ParseError?) GetEntry(int entryStart)
        {
            var charSpan = _reader.PeekChar();

            if ('#' == charSpan)
            {
                IEntry entry = new Junk();
                if (TryGetComment(out var comment, out var error))
                {
                    entry = comment;
                }

                return (entry, error);
            }

            if ('-' == charSpan)
            {
                return GetTerm(entryStart);
            }

            return GetMessage(entryStart);
        }

        private (IEntry, ParseError?) GetTerm(int entryStart)
        {
            IEntry entry = new Junk();
            if (!TryExpectChar('-', out var error))
            {
                return (entry, error);
            }

            if (!TryGetIdentifier(out var id, out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankInline();
            if (!TryExpectChar('=', out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankInline();

            if (!TryGetPattern(out var value, out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankBlock();

            var attribute = GetAttributes();

            if (value != null)
            {
                entry = new AstTerm(id, value, attribute, null);
                return (entry, error);
            }
            else
            {
                error = ParseError.ExpectedTermField(id, entryStart, _reader.Position, _reader.Row);
                return (entry, error);
            }
        }

        private (IEntry, ParseError?) GetMessage(int entryStart)
        {
            IEntry entry = new Junk();
            if (!TryGetIdentifier(out var id, out var error))
            {
                return (entry, error);
            }

            _reader.SkipBlankInline();
            if (!TryExpectChar('=', out error))
            {
                return (entry, error);
            }


            if (!TryGetPattern(out var pattern, out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankBlock();
            var attrs = GetAttributes();

            if (pattern == null && attrs.Count < 1)
            {
                return (entry, ParseError.ExpectedMessageField(id.Name, entryStart, _reader.Position, _reader.Row));
            }

            return (new AstMessage(id, pattern, attrs, null), null);
        }


        #region CommentSyntax

        private bool TryGetComment([NotNullWhen(true)] out AstComment? comment, out ParseError? error)
        {
            var level = CommentLevel.None;
            var content = new List<ReadOnlyMemory<char>>();

            while (_reader.IsNotEof)
            {
                var lineLevel = GetCommentLevel();
                if (lineLevel == CommentLevel.None)
                {
                    _reader.DecrementRow(1);
                    _reader.Position -= 1;
                    break;
                }

                if (level != CommentLevel.None && lineLevel != level)
                {
                    var lineOffset = (int)lineLevel;

                    _reader.DecrementRow(lineOffset);
                    _reader.Position -= lineOffset;
                    break;
                }

                level = lineLevel;

                if (_reader.IsEof)
                {
                    break;
                }

                if ('\n' == _reader.PeekChar()
                    || ('\r' == _reader.PeekChar() && '\n' == _reader.PeekChar(1)))
                {
                    content.Add(_reader.GetCommentLine());
                }
                else
                {
                    if (!TryExpectChar(' ', out var e))
                    {
                        if (content.Count == 0)
                        {
                            error = e;
                            comment = default!;
                            return false;
                        }

                        var lineOffset = (int)lineLevel;

                        _reader.DecrementRow(lineOffset);
                        _reader.Position -= lineOffset;
                        break;
                    }

                    content.Add(_reader.GetCommentLine());
                }

                _reader.SkipEol();
            }

            comment = new AstComment(level, content);
            error = null;
            return true;
        }


        private CommentLevel GetCommentLevel()
        {
            if (_reader.ReadCharIf('#'))
            {
                if (_reader.ReadCharIf('#'))
                {
                    if (_reader.ReadCharIf('#'))
                    {
                        return CommentLevel.ResourceComment;
                    }

                    return CommentLevel.GroupComment;
                }

                return CommentLevel.Comment;
            }

            return CommentLevel.None;
        }

        #endregion

        #region CommonSyntax

        private bool TryExpectChar(char c, out ParseError? error)
        {
            if (_reader.ReadCharIf(c))
            {
                error = null;
                return true;
            }

            error = ParseError.ExpectedToken(c, _reader.PeekChar(), _reader.Position, _reader.Row);
            return false;
        }

        private bool TryGetIdentifier([NotNullWhen(true)] out Identifier? id, out ParseError? error)
        {
            if (_reader.TryPeekChar(out var c) && c.IsAsciiAlphabetic())
            {
                _reader.Position += 1;
                error = null;
                id = GetUncheckedIdentifier();
                return true;
            }

            error = ParseError.ExpectedCharRange("a-zA-Z", _reader.Position, _reader.Row);
            id = default!;
            return false;
        }

        private Identifier GetUncheckedIdentifier()
        {
            // First character is already checked
            var ptr = _reader.Position;
            while (_reader.TryPeekCharAt(ptr, out var c) && c.IsIdentifier())
            {
                ptr += 1;
            }

            Identifier id = new(_reader.ReadSlice(_reader.Position - 1, ptr));
            _reader.Position = ptr;
            return id;
        }

        private bool TryGetPattern(out Pattern? pattern, out ParseError? error)
        {
            var elements = new List<IPatternElementPlaceholder>();
            TextElementPosition textElementRole;
            int? lastNonBlank = null;
            int? commonIndent = null;

            _reader.SkipBlankInline();

            if (_reader.SkipEol())
            {
                _reader.SkipBlankBlock();
                textElementRole = TextElementPosition.LineStart;
            }
            else
            {
                textElementRole = TextElementPosition.InitialLineStart;
            }

            while (_reader.IsNotEof)
            {
                if (_reader.ReadCharIf('{'))
                {
                    if (textElementRole == TextElementPosition.LineStart)
                    {
                        commonIndent = 0;
                    }

                    if (!TryGetPlaceable(out var exp, out error))
                    {
                        pattern = null;
                        return false;
                    }

                    lastNonBlank = elements.Count;
                    elements.Add(new Placeable(exp));
                    textElementRole = TextElementPosition.Continuation;
                }
                else if (_enableExperimental && '-' == _reader.PeekChar() && textElementRole != TextElementPosition.LineStart)
                {
                    _reader.Position += 1;
                    if (_reader.TryPeekChar(out var c) && c.IsAsciiAlphabetic())
                    {
                        _reader.Position += 1;
                        var id = GetUncheckedIdentifier();
                        if (!TryGetAttributeAccessor(out var attribute, out error))
                        {
                            pattern = null;
                            return false;
                        }

                        if (!TryCallArguments(out var args, out error))
                        {
                            pattern = null;
                            return false;
                        }

                        elements.Add(new Placeable(new TermReference(id, attribute, args)));
                    }
                }
                else
                {
                    var sliceStart = _reader.Position;
                    var indent = 0;
                    if (textElementRole == TextElementPosition.LineStart)
                    {
                        indent = _reader.SkipBlankInline();
                        if (_reader.TryPeekChar(out var b))
                        {
                            if (indent == 0)
                            {
                                if ('\r' != b && '\n' != b)
                                {
                                    break;
                                }
                            }
                            else if (b.IsOneOf('.', '}', '[', '*'))
                            {
                                _reader.Position = sliceStart;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!TryGetTextSlice(out var text, out error))
                    {
                        pattern = null;
                        return false;
                    }

                    if (text.Start != text.End)
                    {
                        if (textElementRole == TextElementPosition.LineStart
                            && text.ElementType == TextElementType.NonBlank)
                        {
                            if (commonIndent != null)
                            {
                                if (indent < commonIndent)
                                {
                                    commonIndent = indent;
                                }
                            }
                            else
                            {
                                commonIndent = indent;
                            }
                        }

                        if (textElementRole != TextElementPosition.LineStart
                            || text.ElementType == TextElementType.NonBlank
                            || text.TerminationReason == TextElementTermination.LF)
                        {
                            if (text.ElementType == TextElementType.NonBlank)
                            {
                                lastNonBlank = elements.Count;
                            }

                            elements.Add(new TextElementPlaceholder(
                                sliceStart,
                                text.End,
                                indent,
                                textElementRole,
                                text.TerminationReason == TextElementTermination.CRLF
                            ));
                        }
                    }
                    // In case an empty newline is emitted, we create an artificial token to represent LINEFEED (`\n`)
                    else if (text.Start == text.End && text.TerminationReason == TextElementTermination.CRLF)
                    {
                        elements.Add(new TextElementPlaceholder(
                            0,
                            0,
                            indent,
                            textElementRole,
                            true));
                    }

                    textElementRole = text.TerminationReason switch
                    {
                        TextElementTermination.LF => TextElementPosition.LineStart,
                        TextElementTermination.CRLF => TextElementPosition.LineStart,
                        TextElementTermination.PlaceableStart => TextElementPosition.Continuation,
                        TextElementTermination.EndOfFile => TextElementPosition.Continuation,
                        _ => textElementRole
                    };
                }
            }

            if (lastNonBlank != null)
            {
                List<IPatternElement> patterns = new(lastNonBlank.Value + 1);

                for (int i = 0; i < lastNonBlank + 1; i++)
                {
                    if (i < elements.Count)
                    {
                        var elem = elements[i];
                        if (elem is Placeable placeable)
                        {
                            patterns.Add(placeable);
                        }
                        else if (elem is TextElementPlaceholder textLiteral)
                        {
                            int start = textLiteral.Start;
                            int indent = textLiteral.Indent;
                            var end = textLiteral.End;
                            if (textLiteral.Role == TextElementPosition.LineStart)
                            {
                                if (commonIndent == null)
                                {
                                    start += indent;
                                }
                                else
                                {
                                    start += Math.Min(indent, commonIndent.Value);
                                }
                            }

                            ReadOnlyMemory<char> value;
                            if (textLiteral.MissingEol && textLiteral.Start == textLiteral.End)
                            {
                                value = Cr.AsMemory();
                            }
                            else if (textLiteral.MissingEol && textLiteral.Start != textLiteral.End)
                            {
                                var str = _reader.ReadSlice(start, end) + Cr;
                                value = str.AsMemory();
                            }
                            else
                            {
                                value = _reader.ReadSlice(start, end);
                            }

                            if (lastNonBlank == i)
                            {
#if NETSTANDARD2_1 || UNITY_2021_3_OR_NEWER
                                value = value.TrimEndPolyFill();
#else
                                value = value.TrimEnd();
#endif
                            }

                            patterns.Add(new TextLiteral(value));
                        }
                    }
                }

                pattern = new Pattern(patterns);
                error = null;
                return true;
            }

            // We successfully found nothing
            pattern = null;
            error = null;
            return true;
        }

        private bool TryGetTextSlice([NotNullWhen(true)] out TextSlice? textElement, out ParseError? error)
        {
            var startPos = _reader.Position;
            var textElementType = TextElementType.Blank;

            int index;
            while ((index = _reader.IndexOfAnyChar(" \n\r{}".AsSpan())) != -1)
            {
                if (index > 0)
                {
                    textElementType = TextElementType.NonBlank;
                }

                _reader.Position += index;
                var c = _reader.CurrentChar();

                if (' ' == c)
                {
                    _reader.Position += 1;
                }
                else if ('\n' == c)
                {
                    _reader.Position += 1;
                    _reader.Row += 1;

                    textElement = new TextSlice(
                        startPos,
                        _reader.Position,
                        textElementType,
                        TextElementTermination.LF
                    );
                    error = null;
                    return true;
                }
                else if ('\r' == c
                         && '\n' == _reader.PeekChar(1))
                {
                    // If this is one `/r/n` (CRLF) line ending we take position before CRLF
                    // and set flag in TextElementPlaceholder that value is missing a EOL mark (which is LF)
                    textElement = new TextSlice(
                        startPos,
                        _reader.Position,
                        textElementType,
                        TextElementTermination.CRLF
                    );

                    _reader.Position += 2;
                    _reader.Row += 1;

                    error = null;
                    return true;
                }
                else if ('{' == c)
                {
                    textElement = new TextSlice(
                        startPos,
                        _reader.Position,
                        textElementType,
                        TextElementTermination.PlaceableStart
                    );
                    error = null;
                    return true;
                }
                else if ('}' == c)
                {
                    textElement = null;
                    error = ParseError.UnbalancedClosingBrace(_reader.Position, _reader.Row);
                    return false;
                }
                else
                {
                    textElementType = TextElementType.NonBlank;
                    _reader.Position += 1;
                }
            }

            if (_reader.Position < _reader.GetData.Length)
            {
                textElementType = TextElementType.NonBlank;
                _reader.Position = _reader.GetData.Length;
            }

            textElement = new TextSlice(
                startPos,
                _reader.Position,
                textElementType,
                TextElementTermination.EndOfFile
            );
            error = null;
            return true;
        }

        #endregion

        #region AttributeSyntax

        private List<Attribute> GetAttributes()
        {
            var attributes = new List<Attribute>();

            while (true)
            {
                var lineStart = _reader.Position;
                _reader.SkipBlankInline();
                if (!_reader.ReadCharIf('.'))
                {
                    _reader.Position = lineStart;
                    break;
                }

                if (TryGetAttribute(out var attr))
                {
                    attributes.Add(attr);
                }
                else
                {
                    _reader.Position = lineStart;
                    break;
                }
            }

            return attributes;
        }

        private bool TryGetAttribute([NotNullWhen(true)] out Attribute? attr)
        {
            if (!TryGetIdentifier(out var id, out _))
            {
                attr = default!;
                return false;
            }

            _reader.SkipBlankInline();

            if (!TryExpectChar('=', out _))
            {
                attr = default!;
                return false;
            }

            if (TryGetPattern(out var pattern, out _) && pattern != null)
            {
                attr = new Attribute(id, pattern);
                return true;
            }

            ParseError.MissingValue(_reader.Position, _reader.Row);
            attr = default!;
            return false;
        }

        #endregion

        #region ExpressionSyntax

        private bool TryGetPlaceable([NotNullWhen(true)] out IExpression? expr, out ParseError? error)
        {
            _reader.SkipBlank();
            if (!TryGetExpression(out expr, out error))
            {
                return false;
            }

            _reader.SkipBlankInline();
            if (!TryExpectChar('}', out error))
            {
                expr = null;
                return false;
            }

            if (expr is TermReference { Attribute: { } })
            {
                expr = null;
                error = ParseError.TermAttributeAsPlaceable(_reader.Position, _reader.Row);
                return false;
            }

            return true;
        }

        private bool TryGetExpression([NotNullWhen(true)] out IExpression? retVal, out ParseError? error)
        {
            if (!TryGetInlineExpression(false, out var inlineExpression, out error))
            {
                retVal = inlineExpression;
                return false;
            }

            _reader.SkipBlank();

            if ('-' != _reader.PeekChar()
                || '>' != _reader.PeekChar(1))
            {
                if (inlineExpression is TermReference { Attribute: { } })
                {
                    error = ParseError.TermAttributeAsPlaceable(_reader.Position, _reader.Row);
                    retVal = null;
                    return false;
                }

                retVal = inlineExpression;
                return true;
            }

            if (inlineExpression is MessageReference msgRef)
            {
                if (msgRef.Attribute == null)
                {
                    error = ParseError.MessageReferenceAsSelector(_reader.Position, _reader.Row);
                    retVal = null;
                    return false;
                }

                error = ParseError.MessageAttributeAsSelector(_reader.Position, _reader.Row);
                retVal = null;
                return false;
            }

            if (inlineExpression is not TermReference termRef)
            {
                if (inlineExpression is not TextLiteral
                    && inlineExpression is not NumberLiteral
                    && inlineExpression is not VariableReference
                    && inlineExpression is not FunctionReference
                    && (!_enableExperimental || inlineExpression is not DynamicReference))
                {
                    retVal = null;
                    error = ParseError.ExpectedSimpleExpressionAsSelector(_reader.Position, _reader.Row);
                    return false;
                }
            }
            else
            {
                if (termRef.Attribute == null)
                {
                    retVal = null;
                    error = ParseError.TermReferenceAsSelector(_reader.Position, _reader.Row);
                    return false;
                }
            }

            // We found `->`
            _reader.Position += 2;

            _reader.SkipBlankInline();
            if (!_reader.SkipEol())
            {
                error = ParseError.ExpectedCharRange(@"\n | \r\n", _reader.Position, _reader.Row);
                retVal = null;
                return false;
            }

            _reader.SkipBlank();

            if (!TryGetVariants(out List<Variant> variants, out error))
            {
                retVal = null;
                return false;
            }

            retVal = new SelectExpression(inlineExpression, variants);
            error = null;
            return true;
        }

        private bool TryGetVariants(out List<Variant> variants, out ParseError? error)
        {
            variants = new List<Variant>();
            var hasDefault = false;

            while (true)
            {
                var isDefault = _reader.ReadCharIf('*');
                if (isDefault)
                {
                    if (hasDefault)
                    {
                        error = ParseError.MultipleDefaultVariants(_reader.Position, _reader.Row);
                        return true;
                    }

                    hasDefault = true;
                }

                if (!_reader.ReadCharIf('['))
                {
                    break;
                }

                if (!TryGetVariantKey(out var variant, out error))
                {
                    return false;
                }

                if (!TryGetPattern(out var value, out error))
                {
                    return false;
                }


                if (value != null)
                {
                    variant.InternalValue = value;
                    variant.InternalDefault = isDefault;
                    variants.Add(variant);
                    _reader.SkipBlank();
                }
                else
                {
                    error = ParseError.MissingValue(_reader.Position, _reader.Row);
                    return false;
                }
            }

            if (hasDefault)
            {
                error = null;
                return true;
            }

            error = ParseError.MissingDefaultVariant(_reader.Position, _reader.Row);
            return false;
        }

        private bool TryGetVariantKey([NotNullWhen(true)] out Variant? variantKey, out ParseError? error)
        {
            _reader.SkipBlank();
            VariantType variantType;


            if (_reader.TryPeekChar(out var c) && c.IsNumberStart())
            {
                variantType = VariantType.NumberLiteral;
                if (!TryGetNumberLiteral(out var num, out error))
                {
                    variantKey = null;
                    return false;
                }

                variantKey = new Variant(variantType, num);
            }
            else
            {
                variantType = VariantType.Identifier;
                if (!TryGetIdentifier(out var id, out error))
                {
                    variantKey = null;
                    return false;
                }

                variantKey = new Variant(variantType, id.Name);
            }

            _reader.SkipBlank();
            if (!TryExpectChar(']', out error))
            {
                return false;
            }


            return true;
        }

        private bool TryGetInlineExpression(bool onlyLiteral, [NotNullWhen(true)] out IInlineExpression? expr,
            out ParseError? error)
        {
            if (_reader.TryPeekChar(out var peekChr))
            {
                if ('"' == peekChr)
                {
                    _reader.Position += 1;
                    var start = _reader.Position;
                    while (true)
                    {
                        var b = _reader.PeekChar();
                        if ('\\' == b)
                        {
                            if (_reader.TryPeekCharAt(_reader.Position + 1, out var c))
                            {
                                if (c.IsOneOf('\\', '{', '"'))
                                {
                                    _reader.Position += 2;
                                }
                                else if ('u' == c)
                                {
                                    _reader.Position += 2;
                                    if (!TrySkipUnicodeSequence(4, out error))
                                    {
                                        expr = null;
                                        return false;
                                    }
                                }
                                else if ('U' == c)
                                {
                                    _reader.Position += 2;
                                    if (!TrySkipUnicodeSequence(6, out error))
                                    {
                                        expr = null;
                                        return false;
                                    }
                                }
                                else
                                {
                                    error = ParseError.UnknownEscapeSequence(c, _reader.Position, _reader.Row);
                                    expr = null;
                                    return false;
                                }
                            }
                        }
                        else if ('"' == b)
                        {
                            break;
                        }
                        else if ('\n' == b)
                        {
                            error = ParseError.UnterminatedStringLiteral(_reader.Position, _reader.Row);
                            expr = null;
                            return false;
                        }
                        else
                        {
                            _reader.Position += 1;
                        }
                    }

                    if (!TryExpectChar('"', out error))
                    {
                        expr = null;
                        return false;
                    }

                    var slice = _reader.ReadSlice(start, _reader.Position - 1);
                    expr = new TextLiteral(slice);
                    return true;
                }

                if (peekChr.IsAsciiDigit())
                {
                    if (!TryGetNumberLiteral(out var num, out error))
                    {
                        expr = null;
                        return false;
                    }

                    expr = new NumberLiteral(num);
                    return true;
                }

                if ('-' == peekChr && !onlyLiteral)
                {
                    _reader.Position += 1;
                    if (_reader.TryPeekChar(out var c) && c.IsAsciiAlphabetic())
                    {
                        _reader.Position += 1;
                        var id = GetUncheckedIdentifier();
                        if (!TryGetAttributeAccessor(out var attribute, out error))
                        {
                            expr = null;
                            return false;
                        }

                        if (!TryCallArguments(out var args, out error))
                        {
                            expr = null;
                            return false;
                        }

                        expr = new TermReference(id, attribute, args);
                        return true;
                    }
                    else
                    {
                        _reader.Position -= 1;
                        if (TryGetNumberLiteral(out var num, out error))
                        {
                            expr = new NumberLiteral(num);
                            return true;
                        }

                        expr = null;
                        return false;
                    }
                }
                if (_enableExperimental && '$' == peekChr && _reader.PeekChar(1) == '$')
                {
                    _reader.Position += 3;
                    return TryGetDynamicReference(out expr, out error);
                }
                else if ('$' == peekChr && !onlyLiteral)
                {
                    _reader.Position += 1;
                    if (!TryGetIdentifier(out var id, out error))
                    {
                        expr = null;
                        return false;
                    }

                    expr = new VariableReference(id);
                    return true;
                }
                else if (peekChr.IsAsciiAlphabetic())
                {
                    _reader.Position += 1;
                    var id = GetUncheckedIdentifier();

                    if (!TryCallArguments(out var args, out error))
                    {
                        expr = null;
                        return false;
                    }

                    if (args != null)
                    {
                        if (!id.Name.Span.IsCallee())
                        {
                            error = ParseError.ForbiddenCallee(_reader.Position, _reader.Row);
                            expr = null;
                            return false;
                        }

                        error = null;
                        expr = new FunctionReference(id, args.Value);
                        return true;
                    }

                    else
                    {
                        if (!TryGetAttributeAccessor(out var attribute, out error))
                        {
                            expr = null;
                            return false;
                        }

                        expr = new MessageReference(id, attribute);
                        return true;
                    }
                }
                else if ('{' == peekChr && !onlyLiteral)
                {
                    _reader.Position += 1;
                    if (!TryGetPlaceable(out var exp, out error))
                    {
                        expr = null;
                        return false;
                    }

                    error = null;
                    expr = new Placeable(exp);
                    return true;
                }
            }

            if (onlyLiteral)
            {
                error = ParseError.ExpectedLiteral(_reader.Position, _reader.Row);
                expr = null;
                return false;
            }

            error = ParseError.ExpectedInlineExpression(_reader.Position, _reader.Row);
            expr = null;
            return false;
        }

        private bool TryGetDynamicReference(out IInlineExpression? expr, out ParseError? error)
        {
            var id = GetUncheckedIdentifier();

            if (!TryCallArguments(out var args, out error))
            {
                expr = null;
                return false;
            }

            if (!TryGetAttributeAccessor(out var attribute, out error))
            {
                expr = null;
                return false;
            }

            expr = new DynamicReference(id, attribute, args);
            return true;
        }

        private bool TryCallArguments(out CallArguments? args, out ParseError? error, bool onlyLiteral = false)
        {
            _reader.SkipBlank();
            if (!_reader.ReadCharIf('('))
            {
                args = null;
                error = null;
                return true;
            }

            var positional = new List<IInlineExpression>();
            var nameArgs = new List<NamedArgument>();
            var argNames = new List<Identifier>();

            _reader.SkipBlank();

            while (_reader.IsNotEof)
            {
                if (')' == _reader.PeekChar())
                {
                    break;
                }

                if (!TryGetArgument(out error, argNames, nameArgs, positional, onlyLiteral))
                {
                    args = null;
                    return false;
                }

                _reader.SkipBlank();
                if (!(_reader.TryPeekChar(out var c) && c.IsOneOf(',', ')')))
                {
                    args = new CallArguments(positional, nameArgs);
                    error = ParseError.ExpectedToken(',', ')', _reader.PeekChar(), _reader.Position, _reader.Row);
                    return false;
                }

                _reader.ReadCharIf(',');
                _reader.SkipBlank();
            }

            if (!TryExpectChar(')', out error))
            {
                args = null;
                return false;
            }

            args = new CallArguments(positional, nameArgs);
            return true;
        }

        private bool TryGetArgument(out ParseError? error,
            List<Identifier> argNames, List<NamedArgument> nameArgs,
            List<IInlineExpression> positional, bool onlyLiteral = false)
        {
            if (!TryGetInlineExpression(onlyLiteral, out var expr, out error))
            {
                return false;
            }

            if (expr is MessageReference { Attribute: null } msgRef)
            {
                var id = msgRef.Id;
                _reader.SkipBlank();
                if (':' == _reader.PeekChar())
                {
                    if (argNames.Contains(id))
                    {
                        error = ParseError.DuplicatedNamedArgument(id, _reader.Position, _reader.Row);
                        return false;
                    }

                    _reader.Position += 1;
                    _reader.SkipBlank();

                    if (!TryGetInlineExpression(onlyLiteral, out var val, out error))
                    {
                        return false;
                    }

                    argNames.Add(id);
                    nameArgs.Add(new NamedArgument(id, val));
                }
                else
                {
                    if (argNames.Count > 0)
                    {
                        error = ParseError.PositionalArgumentFollowsNamed(_reader.Position, _reader.Row);
                        return false;
                    }

                    positional.Add(expr);
                }
            }
            else
            {
                if (argNames.Count > 0)
                {
                    error = ParseError.PositionalArgumentFollowsNamed(_reader.Position, _reader.Row);
                    return false;
                }

                positional.Add(expr);
            }

            return true;
        }

        private bool TryGetAttributeAccessor(out Identifier? id, out ParseError? error)
        {
            if (_reader.ReadCharIf('.'))
            {
                if (!TryGetIdentifier(out id, out error))
                {
                    return false;
                }

                return true;
            }

            id = null;
            error = null;
            return true;
        }

        private bool TryGetNumberLiteral(out ReadOnlyMemory<char> num, out ParseError? error)
        {
            var start = _reader.Position;
            _reader.ReadCharIf('-');
            if (!TrySkipDigits(out error))
            {
                num = null;
                return false;
            }

            if (_reader.ReadCharIf('.') && !TrySkipDigits(out error))
            {
                num = null;
                return false;
            }

            num = _reader.ReadSlice(start, _reader.Position);
            error = null;
            return true;
        }

        private bool TrySkipDigits(out ParseError? error)
        {
            var start = _reader.Position;
            while (_reader.TryPeekChar(out var c) && c.IsAsciiDigit())
            {
                _reader.Position += 1;
            }

            if (start == _reader.Position)
            {
                error = ParseError.ExpectedCharRange("0-9", _reader.Position, _reader.Row);
                return false;
            }

            error = null;
            return true;
        }

        private bool TrySkipUnicodeSequence(int length, out ParseError? error)
        {
            var start = _reader.Position;
            for (int i = 0; i < length; i++)
            {
                if (_reader.TryPeekChar(out var c) && c.IsAsciiHexdigit())
                {
                    _reader.Position += 1;
                }
                else
                {
                    break;
                }
            }

            if (_reader.Position - start != length)
            {
                var end = _reader.IsEof ? _reader.Position : _reader.Position + 1;
                var seq = _reader.ReadSliceToStr(start, end);
                error = ParseError.InvalidUnicodeEscapeSequence(seq, _reader.Position, _reader.Row);
                return false;
            }

            error = null;
            return true;
        }

        #endregion
    }

    public class TextSlice
    {
        public int Start { get; }
        public int End { get; }
        public TextElementType ElementType { get; }
        public TextElementTermination TerminationReason { get; }

        public TextSlice(int start, int end, TextElementType elementType, TextElementTermination terminationReason)
        {
            Start = start;
            End = end;
            ElementType = elementType;
            TerminationReason = terminationReason;
        }
    }
}

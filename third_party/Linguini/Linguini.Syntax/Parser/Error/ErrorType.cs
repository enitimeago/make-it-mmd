namespace Linguini.Syntax.Parser.Error
{
    public enum ErrorType : byte
    {
        ExpectedToken,
        ExpectedCharRange,
        ExpectedMessageField,
        MissingValue,
        UnbalancedClosingBrace,
        TermAttributeAsPlaceable,
        ExpectedTermField,
        MessageReferenceAsSelector,
        MessageAttributeAsSelector,
        TermReferenceAsSelector,
        ExpectedSimpleExpressionAsSelector,
        UnterminatedStringLiteral,
        UnknownEscapeSequence,
        ForbiddenCallee,
        ExpectedLiteral,
        ExpectedInlineExpression,
        DuplicatedNamedArgument,
        PositionalArgumentFollowsNamed,
        MultipleDefaultVariants,
        InvalidUnicodeEscapeSequence
    }
}

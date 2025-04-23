using System.Text;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Tests.Parser
{
    public static class AstDebugHelper
    {
        public static string Debug(this AstMessage message)
        {
            var stringBuilder = new StringBuilder();
            if (message.Value != null)
            {
                foreach (var patternElement in message.Value.Elements)
                {
                    switch (patternElement)
                    {
                        case TextLiteral textLiteral:
                            stringBuilder.Append((object)textLiteral.Value);
                            break;
                        case Placeable placeable:
                            Debug(placeable, stringBuilder);
                            break;
                    }
                } 
            }
            
            return stringBuilder.ToString();
        }

        private static void Debug(Placeable placeable, StringBuilder stringBuilder)
        {
            switch (placeable.Expression)
            {
                case SelectExpression selectExpression:
                    Debug(selectExpression, stringBuilder);
                    break;
                case IInlineExpression inlineExpression:
                    Debug(inlineExpression, stringBuilder);
                    break;
            }
        }

        private static void Debug(IInlineExpression inlineExpression, StringBuilder stringBuilder)
        {
            throw new System.NotImplementedException();
        }

        private static void Debug(SelectExpression selectExpression, StringBuilder stringBuilder)
        {
            throw new System.NotImplementedException();
        }
    }
}
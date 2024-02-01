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
using System.Text;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Ast;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Syntax.Tests.Parser
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
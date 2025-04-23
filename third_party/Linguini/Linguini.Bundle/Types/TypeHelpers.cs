using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Types
{
    public static class TypeHelpers
    {
        public static Attribute? GetAttribute(this AstMessage message, string attrName)
        {
            return message.Attributes
                .Find(attribute => attribute.Id.ToString() == attrName);
        }

        public static FluentNumber? ToFluentNumber(this IFluentType fluentType)
        {
            if (fluentType is FluentNumber type)
            {
                return type;
            }

            return null;
        }
    }
}
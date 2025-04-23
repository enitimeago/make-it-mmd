using System;
using System.Collections.Generic;
using System.Text;

namespace Linguini.Bundle.Errors
{
    public class LinguiniException : Exception
    {
        public LinguiniException(IList<FluentError> errors) 
            : this(FluentErrorsToString(errors))
        {
        }

        private static string FluentErrorsToString(IList<FluentError> errors)
        {
            StringBuilder sb = new();
            sb.Append("Following errors weren't handled:\n");
            foreach (var error in errors)
            {
                sb.Append(error).Append('\n');
            }
            return sb.ToString();
        }

        private LinguiniException(string name) : base(name) {}
    }
}

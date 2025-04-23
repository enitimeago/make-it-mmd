using System.Collections.Generic;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Types
{
    using FluentArgs = IDictionary<string, IFluentType>;

    public delegate IFluentType ExternalFunction(
        IList<IFluentType> positionalArgs,
        FluentArgs namedArgs);
}

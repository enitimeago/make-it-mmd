namespace Linguini.Bundle.Types
{
    public record FluentFunction(ExternalFunction Function)
    {
        public static implicit operator FluentFunction(ExternalFunction ef) => new(ef);
    }
}

using Linguini.Shared.Util;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent value representing lack of value
    /// similar to <c>Nullable</c> or <c>Option</c> monad. 
    /// </summary>
    public record FluentNone : IFluentType
    {
        /// <summary>
        /// Static value representing empty value.
        /// </summary>
        public static readonly FluentNone None = new();
        
        private FluentNone()
        {
        }

        public bool Matches(IFluentType other, IScope scope)
        {
            return SharedUtil.Matches(this, other, scope);
        }

        /// <inheritdoc/>
        public IFluentType Copy()
        {
            return None;
        }

        /// <inheritdoc/>
        public string AsString()
        {
            return "{???}";
        }

        public bool IsError()
        {
            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return AsString();
        }
    }
}

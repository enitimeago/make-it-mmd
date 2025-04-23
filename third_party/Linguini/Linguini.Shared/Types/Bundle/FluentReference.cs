using System;
using Linguini.Shared.Util;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent representation of a string value
    /// </summary>
    public record FluentReference : IFluentType
    {
        private readonly string _reference = "";
        
        public FluentReference(string reference)
        {
            _reference = reference;
        }
        
        public FluentReference(ReadOnlySpan<char> reference)
        {
            _reference = reference.ToString();
        }
        public string AsString()
        {
            return _reference;
        }

        public bool IsError()
        {
            return false;
        }

        public bool Matches(IFluentType other, IScope scope)
        {
            return SharedUtil.Matches(this, other, scope);
        }

        public IFluentType Copy()
        {
            return new FluentReference(_reference);
        }
        
        public static implicit operator string(FluentReference fs) => fs._reference;
        public static implicit operator FluentReference(string s) => new(s);
    }
}
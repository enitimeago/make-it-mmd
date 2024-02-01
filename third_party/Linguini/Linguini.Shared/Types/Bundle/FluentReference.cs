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
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Util;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle
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
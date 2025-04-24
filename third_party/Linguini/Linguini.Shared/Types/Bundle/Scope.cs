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
namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Common interface used for Fluent Bundle scopes
    /// </summary>
    public interface IScope
    {
        /// <summary>
        /// Method used for getting plural category of a number
        /// </summary>
        /// <param name="type">Whether we treat number as a cardinal (one, two, three, etc.) or ordinal (first,
        /// second, third, etc.) </param>
        /// <param name="number">The number for which we get plural category</param>
        /// <returns>Plural category of the number for given RuleType</returns>
        PluralCategory GetPluralRules(RuleType type, FluentNumber number);
    }

    public static class ScopeHelper
    {
        /// <summary>
        /// Method that compares a Fluent string to a Fluent number
        /// </summary>
        /// <param name="scope">Scope of Fluent Bundle</param>
        /// <param name="fs1"></param>
        /// <param name="fn2"></param>
        /// <returns></returns>
        public static bool MatchByPluralCategory(this IScope scope, FluentString fs1, FluentNumber fn2)
        {
            if (!fs1.TryGetPluralCategory(out var strCategory)) return false;
            var numCategory = scope.GetPluralRules(RuleType.Cardinal, fn2);
        
            return numCategory == strCategory;
        }
    }
}

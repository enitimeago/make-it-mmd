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
using System.Diagnostics.CodeAnalysis;

namespace enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types
{
    /// <summary>
    /// Plural category according to CLDR Plural Category <seealso href="https://www.unicode.org/cldr/cldr-aux/charts/30/supplemental/language_plural_rules.html">(link)</seealso>
    /// </summary>
    public enum PluralCategory : byte
    {
        Zero,
        One,
        Two,
        Few,
        Many,
        Other,
    }

    public static class PluralCategoryHelper
    {
        /// <summary>
        /// Try to convert a string to to a Plural category. 
        /// </summary>
        /// <param name="input">Case insensitive name of the Plural category</param>
        /// <param name="pluralCategory">found Plural category if returns <c>true</c>, or <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if it matches the <c>pluralCategory</c> value</returns>
        public static bool TryPluralCategory(this string? input, [NotNullWhen(true)] out PluralCategory? pluralCategory)
        {
            if (input != null)
            {
                switch (input.ToLower())
                {
                    case "zero":
                        pluralCategory = PluralCategory.Zero;
                        return true;
                    case "one":
                        pluralCategory = PluralCategory.One;
                        return true;
                    case "two":
                        pluralCategory = PluralCategory.Two;
                        return true;
                    case "few":
                        pluralCategory = PluralCategory.Few;
                        return true;
                    case "many":
                        pluralCategory = PluralCategory.Many;
                        return true;
                    case "other" or "default":
                        pluralCategory = PluralCategory.Other;
                        return true;
                }
            }

            pluralCategory = null;
            return false;
        }
    }
}

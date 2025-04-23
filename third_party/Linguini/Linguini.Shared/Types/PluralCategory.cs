using System.Diagnostics.CodeAnalysis;

namespace Linguini.Shared.Types
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

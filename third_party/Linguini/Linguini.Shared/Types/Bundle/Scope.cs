namespace Linguini.Shared.Types.Bundle
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

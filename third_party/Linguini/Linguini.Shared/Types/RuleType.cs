namespace Linguini.Shared.Types
{
    /// <summary>
    /// Which type of rule matching should be performed
    /// </summary>
    public enum RuleType : byte
    {
        /// <summary>
        /// Ordinal - number is used in ordering
        /// e.g. first, second, third, etc.
        /// </summary>
        Ordinal,
        /// <summary>
        /// Cardinal - number type generally used.
        /// e.g. one, two, three, etc.
        /// </summary>
        Cardinal,
    }
}

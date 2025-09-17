using WOWCAU.Core.Parts.Helper.Contracts;

namespace WOWCAU.Core.Parts.Helper.Defaults
{
    public sealed class PluralizeHelper : IPluralizeHelper
    {
        public string PluralizeWord(string word)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(word);

            var lastCharIsUpperCase = char.IsUpper(word.Last());

            if (word.Equals("entry", StringComparison.CurrentCultureIgnoreCase))
            {
                return lastCharIsUpperCase ? word.Replace("Y", "IES") : word.Replace("y", "ies");
            }

            return lastCharIsUpperCase ? word + "S" : word + "s";
        }

        public string PluralizeWord(string word, Func<bool> predicate)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(word);
            ArgumentNullException.ThrowIfNull(predicate);

            return predicate.Invoke() ? PluralizeWord(word) : word;
        }
    }
}

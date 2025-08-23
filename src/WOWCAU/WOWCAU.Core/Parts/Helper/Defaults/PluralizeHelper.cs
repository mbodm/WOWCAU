using WOWCAU.Core.Parts.Helper.Contracts;

namespace WOWCAU.Core.Parts.Helper.Defaults
{
    public sealed class PluralizeHelper : IPluralizeHelper
    {
        public string PluralizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentException($"'{nameof(word)}' cannot be null or whitespace.", nameof(word));
            }

            return word.All(char.IsUpper) ? word + "S" : word + "s";
        }

        public string PluralizeWord(string word, Func<bool> predicate)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentException($"'{nameof(word)}' cannot be null or whitespace.", nameof(word));
            }

            ArgumentNullException.ThrowIfNull(predicate);

            return predicate.Invoke() ? PluralizeWord(word) : word;
        }
    }
}

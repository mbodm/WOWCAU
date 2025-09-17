namespace WOWCAU.Core.Parts.Helper.Contracts
{
    public interface IPluralizeHelper
    {
        string PluralizeWord(string word);
        string PluralizeWord(string word, Func<bool> predicate);
    }
}

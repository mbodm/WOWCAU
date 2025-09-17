namespace WOWCAU.Core.Parts.Modules.Update.Contracts
{
    public interface IAppRestarter
    {
        void RestartApplication(uint delayInSeconds);
        Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default);
    }
}

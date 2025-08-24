namespace WOWCAU.Helper.Parts.Contracts
{
    public interface IProcessHelper
    {
        bool IsRunningProcess(string exeFilePath);
        Task<bool> KillProcessAsync(string exeFilePath, CancellationToken cancellationToken = default);
        Task<bool> StartIndependentProcessAsync(string exeFilePath, CancellationToken cancellationToken = default);
    }
}

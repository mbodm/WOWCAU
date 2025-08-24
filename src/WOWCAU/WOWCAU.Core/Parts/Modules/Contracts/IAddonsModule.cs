namespace WOWCAU.Core.Parts.Modules.Contracts
{
    public interface IAddonsModule
    {
        Task<uint> ProcessAddonsAsync(IProgress<byte>? progress = null, CancellationToken cancellationToken = default);
    }
}

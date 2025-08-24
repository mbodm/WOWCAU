namespace WOWCAU.Core.Parts.Domain.Modules.Contracts
{
    public interface IAddonsModule
    {
        Task<uint> ProcessAddonsAsync(IProgress<byte>? progress = null, CancellationToken cancellationToken = default);
    }
}

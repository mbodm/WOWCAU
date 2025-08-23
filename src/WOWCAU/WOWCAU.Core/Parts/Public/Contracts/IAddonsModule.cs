namespace WOWCAU.Core.Parts.Public.Contracts
{
    public interface IAddonsModule
    {
        Task<uint> ProcessAddonsAsync(IProgress<byte>? progress = null, CancellationToken cancellationToken = default);
    }
}

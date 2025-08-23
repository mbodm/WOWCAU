using WOWCAU.Core.Parts.Domain.Config.Types;

namespace WOWCAU.Core.Parts.Domain.Config.Contracts
{
    public interface IConfigReader
    {
        Task<ConfigData> ReadAsync(CancellationToken cancellationToken = default);
    }
}

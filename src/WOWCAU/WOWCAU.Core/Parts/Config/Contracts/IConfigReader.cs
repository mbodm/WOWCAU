using WOWCAU.Core.Parts.Config.Types;

namespace WOWCAU.Core.Parts.Config.Contracts
{
    public interface IConfigReader
    {
        Task<ConfigData> ReadAsync(CancellationToken cancellationToken = default);
    }
}

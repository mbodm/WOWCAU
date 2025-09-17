using WOWCAU.Core.Parts.Modules.Config.Types;

namespace WOWCAU.Core.Parts.Modules.Config.Contracts
{
    public interface IConfigReader
    {
        Task<ConfigData> ReadAsync(CancellationToken cancellationToken = default);
    }
}

using WOWCAU.Core.Parts.Domain.Logging.Contracts;
using WOWCAU.Core.Parts.Domain.Modules.Contracts;

namespace WOWCAU.Core.Parts
{
    public interface IDomainLogic
    {
        ILogger Logger { get; }
        IAppModule App { get; }
        IAddonsModule Addons { get; }
        IUpdateModule Update { get; }
    }
}

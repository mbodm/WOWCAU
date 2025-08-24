using WOWCAU.Core.Parts.Logging.Contracts;
using WOWCAU.Core.Parts.Modules.Contracts;

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

using WOWCAU.Core.Parts.Domain.Modules.Contracts;
using WOWCAU.Core.Parts.Logging.Contracts;

namespace WOWCAU.Core.Parts.Domain.Outer.Contracts
{
    public interface IDomainLogic
    {
        ILogger Logger { get; }
        IAppModule App { get; }
        IAddonsModule Addons { get; }
        IUpdateModule Update { get; }
    }
}

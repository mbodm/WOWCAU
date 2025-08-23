using WOWCAU.Core.Parts.Domain.Config.Types;

namespace WOWCAU.Core.Parts.Domain.Config.Contracts
{
    public interface IConfigValidator
    {
        public void Validate(ConfigData configData);
    }
}

using WOWCAU.Core.Parts.Modules.Config.Types;

namespace WOWCAU.Core.Parts.Modules.Config.Contracts
{
    public interface IConfigValidator
    {
        public void Validate(ConfigData configData);
    }
}

using WOWCAU.Core.Parts.Config.Types;

namespace WOWCAU.Core.Parts.Config.Contracts
{
    public interface IConfigValidator
    {
        public void Validate(ConfigData configData);
    }
}

namespace WOWCAU.Core.Parts.Modules.Addons.Contracts
{
    public interface ISmartUpdateFeature
    {
        Task LoadAsync(string workFolder, CancellationToken cancellationToken = default);
        Task SaveAsync(string workFolder, CancellationToken cancellationToken = default);
        bool AddonVersionAlreadyExists(string addonName, string downloadUrl, string zipFile);
        void AddOrUpdateAddonVersion(string addonName, string downloadUrl, string zipFile);
    }
}

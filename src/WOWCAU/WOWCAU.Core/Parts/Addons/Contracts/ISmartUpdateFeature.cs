namespace WOWCAU.Core.Parts.Addons.Contracts
{
    public interface ISmartUpdateFeature
    {
        Task LoadAsync(string baseFolder, CancellationToken cancellationToken = default);
        Task SaveAsync(string baseFolder, CancellationToken cancellationToken = default);
        bool AddonVersionAlreadyExists(string addonName, string downloadUrl, string zipFile);
        void AddOrUpdateAddonVersion(string addonName, string downloadUrl, string zipFile);
    }
}

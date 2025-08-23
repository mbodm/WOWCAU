using WOWCAU.Core.Parts.Domain.Logging.Contracts;
using WOWCAU.Core.Parts.Public.Types;

namespace WOWCAU.Core.Parts.Public.Contracts
{
    public interface IAppModule
    {
        ILogger Logger { get; }
        SettingsData Settings { get; }
        string ConfigStorageInformation { get; } // Using such a generic term here since this could be a file/database/whatever

        Task LoadSettingsAsync(CancellationToken cancellationToken = default);
        string GetApplicationVersion();
        void OpenFolderInExplorer(string folder);
        void ShowLogFileInNotepad();
    }
}

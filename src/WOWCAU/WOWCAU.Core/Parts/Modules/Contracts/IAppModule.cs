using WOWCAU.Core.Parts.Modules.Types;

namespace WOWCAU.Core.Parts.Modules.Contracts
{
    public interface IAppModule
    {
        SettingsData Settings { get; }
        string ConfigStorageInformation { get; } // Using such a generic term here since this could be a file/database/whatever

        Task LoadSettingsAsync(CancellationToken cancellationToken = default);
        string GetApplicationVersion();
        void OpenConfigFolderInExplorer();
        void OpenProgramFolderInExplorer();
        void OpenAddonsFolderInExplorer();
        void ShowLogFileInNotepad();
        string PluralizeAddonWord(uint count);
    }
}

using WOWCAU.Core.Parts.Domain.Types;
using WOWCAU.Core.Parts.Helper.Types;
using WOWCAU.Core.Parts.Modules.Update.Types;

namespace WOWCAU.Core.Parts.Domain.Contracts
{
    public interface IDomainLogic
    {
        SettingsData Settings { get; }
        string ConfigStorageInformation { get; } // Using such a generic term here since this could be a file/database/whatever

        // Application
        void LogApplicationStart();
        Task LoadSettingsAsync(CancellationToken cancellationToken = default);
        string GetApplicationVersion();
        void OpenConfigFolderInExplorer();
        void OpenProgramFolderInExplorer();
        void OpenAddonsFolderInExplorer();
        void ShowLogFileInNotepad();
        string PluralizeWordByCount(string singular, uint count);

        // Addons
        Task<uint> ProcessAddonsAsync(IProgress<byte>? progress = null, CancellationToken cancellationToken = default);

        // Update
        Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default);
        Task DownloadUpdateAsync(UpdateData updateData, IProgress<DownloadProgress>? downloadProgress = default, CancellationToken cancellationToken = default);
        Task ApplyUpdateAndRestartApplicationAsync(CancellationToken cancellationToken = default);
        Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default);
    }
}

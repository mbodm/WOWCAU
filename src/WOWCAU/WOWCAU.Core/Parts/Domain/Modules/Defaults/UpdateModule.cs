﻿using WOWCAU.Core.Parts.Domain.Logging.Contracts;
using WOWCAU.Core.Parts.Domain.Modules.Contracts;
using WOWCAU.Core.Parts.Domain.Update.Contracts;
using WOWCAU.Core.Parts.Domain.Update.Types;
using WOWCAU.Core.Parts.Helper.Contracts;
using WOWCAU.Core.Parts.Helper.Types;

namespace WOWCAU.Core.Parts.Domain.Modules.Defaults
{
    public sealed class UpdateModule(ILogger logger, IAppHelper appHelper, IAppModule settingsModule, IUpdateManager updateManager) : IUpdateModule
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAppModule settingsModule = settingsModule ?? throw new ArgumentNullException(nameof(settingsModule));
        private readonly IUpdateManager updateManager = updateManager ?? throw new ArgumentNullException(nameof(updateManager));

        private readonly string appName = appHelper.GetApplicationName();

        public async Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default)
        {
            // For temp folder we trust application settings and config validator (since this is business logic and not a helper) and therefore do no 2nd check here

            var updateFolder = Path.Combine(settingsModule.Settings.TempFolder);
            if (!Directory.Exists(updateFolder))
            {
                Directory.CreateDirectory(updateFolder);
            }

            await updateManager.InitAsync(updateFolder, cancellationToken).ConfigureAwait(false);

            try
            {
                var updateData = await updateManager.CheckForUpdateAsync(cancellationToken).ConfigureAwait(false);

                return updateData;
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException($"Could not determine the latest {appName} version (see log file for details).", e);
            }
        }

        public async Task DownloadUpdateAsync(UpdateData updateData, IProgress<DownloadProgress>? downloadProgress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await updateManager.DownloadUpdateAsync(updateData, downloadProgress, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException($"Error while downloading latest {appName} release (see log file for details).", e);
            }
        }

        public async Task ApplyUpdateAndRestartApplicationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await updateManager.ApplyUpdateAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Could not apply update (see log file for details).", e);
            }

            try
            {
                updateManager.RestartApplication(2);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Error while restarting application (see log file for details).", e);
            }
        }

        public async Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await updateManager.RemoveBakFileIfExistsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Error while removing .bak file of application update (see log file for details).", e);
            }
        }
    }
}

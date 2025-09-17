using System.Diagnostics;
using WOWCAU.Core.Parts.Helper.Contracts;
using WOWCAU.Core.Parts.Modules.System.Contracts;
using WOWCAU.Core.Parts.Modules.Update.Contracts;

namespace WOWCAU.Core.Parts.Modules.Update.Defaults
{
    public sealed class AppRestarter(IAppHelper appHelper, IReliableFileOperations reliableFileOperations) : IAppRestarter
    {
        private readonly IAppHelper appHelper = appHelper ?? throw new ArgumentNullException(nameof(appHelper));
        private readonly IReliableFileOperations reliableFileOperations = reliableFileOperations ?? throw new ArgumentNullException(nameof(reliableFileOperations));

        public void RestartApplication(uint delayInSeconds)
        {
            if (delayInSeconds > 10)
            {
                delayInSeconds = 10;
            }

            // To decouple our .exe call from the cmd.exe process, we also need to use "start" here.
            // Since we could have spaces in our .exe path, the path has to be surrounded by quotes.
            // Doing this properly, together with "start", its fist argument has to be empty quotes.
            // See here -> https://stackoverflow.com/questions/2937569/how-to-start-an-application-without-waiting-in-a-batch-file

            var psi = new ProcessStartInfo
            {
                Arguments = $"/C ping 127.0.0.1 -n {delayInSeconds} && start \"\" \"{appHelper.GetApplicationExecutableFilePath()}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };

            var process = Process.Start(psi) ?? throw new InvalidOperationException("The 'Process.Start()' call returned null.");
        }

        public async Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default)
        {
            var exeFilePath = appHelper.GetApplicationExecutableFilePath();
            var bakFilePath = Path.ChangeExtension(exeFilePath, ".bak");

            if (File.Exists(bakFilePath))
            {
                File.Delete(bakFilePath);
                await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

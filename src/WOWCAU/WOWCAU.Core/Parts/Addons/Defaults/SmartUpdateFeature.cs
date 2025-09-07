using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Linq;
using WOWCAU.Core.Parts.Addons.Contracts;
using WOWCAU.Core.Parts.Addons.Types;
using WOWCAU.Core.Parts.Extensions;
using WOWCAU.Core.Parts.Logging.Contracts;

namespace WOWCAU.Core.Parts.Addons.Defaults
{
    public sealed class SmartUpdateFeature(ILogger logger) : ISmartUpdateFeature
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private readonly ConcurrentDictionary<string, SmartUpdateData> dict = new();

        public async Task LoadAsync(string baseFolder, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(baseFolder);

            logger.LogMethodEntry();

            dict.Clear();

            var xmlFile = GetXmlFile(baseFolder);
            if (!File.Exists(xmlFile))
            {
                return;
            }

            using var fileStream = new FileStream(xmlFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            XDocument doc;
            try
            {
                doc = await XDocument.LoadAsync(fileStream, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Error while loading SmartUpdate file: The file is either empty or not a valid XML file.", e);
            }

            var root = doc.Element("wowcau") ?? throw new InvalidOperationException("Error in SmartUpdate file: The <wowcau> root element not exists.");
            var parent = root.Element("smartupdate") ?? throw new InvalidOperationException("Error in SmartUpdate file: The <smartupdate> section not exists.");

            var entries = parent.Elements("entry");
            foreach (var entry in entries)
            {
                var addonName = entry?.Attribute("addonName")?.Value ?? string.Empty;
                var previousDownloadUrl = entry?.Attribute("previousDownloadUrl")?.Value ?? string.Empty;
                var previousZipFile = entry?.Attribute("previousZipFile")?.Value ?? string.Empty;
                var changedAt = entry?.Attribute("changedAt")?.Value ?? string.Empty;

                if (string.IsNullOrWhiteSpace(addonName) ||
                    string.IsNullOrWhiteSpace(previousDownloadUrl) ||
                    string.IsNullOrWhiteSpace(previousZipFile) ||
                    string.IsNullOrWhiteSpace(changedAt))
                {
                    throw new InvalidOperationException("Error in SmartUpdate file: The <smartupdate> section contains one or more invalid entries.");
                }

                if (!dict.TryAdd(addonName, new SmartUpdateData(addonName, previousDownloadUrl, previousZipFile, changedAt)))
                {
                    throw new InvalidOperationException("Error in SmartUpdate file: The <smartupdate> section contains multiple entries for the same addon.");
                }
            }

            logger.LogMethodExit();
        }

        public async Task SaveAsync(string baseFolder, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(baseFolder);

            logger.LogMethodEntry();

            var entries = dict.OrderBy(kvp => kvp.Key).Select(kvp => new XElement("entry",
                new XAttribute("addonName", kvp.Key),
                new XAttribute("previousDownloadUrl", kvp.Value.DownloadUrl),
                new XAttribute("previousZipFile", kvp.Value.ZipFile),
                new XAttribute("changedAt", kvp.Value.TimeStamp)));

            var doc = new XDocument(new XElement("wowcau", new XElement("smartupdate", entries)));

            var xmlFile = GetXmlFile(baseFolder);
            using var fileStream = new FileStream(xmlFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineOnAttributes = true, Async = true });
            await xmlWriter.FlushAsync().ConfigureAwait(false);
            await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);

            await doc.SaveAsync(xmlWriter, cancellationToken).ConfigureAwait(false);

            logger.LogMethodExit();
        }

        public bool AddonVersionAlreadyExists(string addonName, string downloadUrl, string zipFile)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(addonName);
            ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(zipFile);

            if (!dict.TryGetValue(addonName, out SmartUpdateData? value) || value == null)
            {
                return false;
            }

            var hasExactEntry = value.AddonName == addonName && value.DownloadUrl == downloadUrl && value.ZipFile == zipFile;

            return hasExactEntry;
        }

        public void AddOrUpdateAddonVersion(string addonName, string downloadUrl, string zipFile)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(addonName);
            ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(zipFile);

            if (AddonVersionAlreadyExists(addonName, downloadUrl, zipFile))
            {
                return;
            }

            var timeStamp = DateTime.UtcNow.ToIso8601();
            var dictValue = new SmartUpdateData(addonName, downloadUrl, zipFile, timeStamp);
            dict.AddOrUpdate(addonName, dictValue, (_, _) => dictValue);
        }

        private static string GetXmlFile(string baseFolder) => Path.Combine(baseFolder, "SmartUpdate.xml");
    }
}

﻿using System.Xml.Linq;
using WOWCAU.Core.Parts.Modules.Config.Contracts;
using WOWCAU.Core.Parts.Modules.Config.Types;
using WOWCAU.Core.Parts.Modules.System.Contracts;

namespace WOWCAU.Core.Parts.Modules.Config.Defaults
{
    public sealed class XmlConfigReader(ILogger logger, IConfigStorage storage) : IConfigReader
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IConfigStorage storage = storage ?? throw new ArgumentNullException(nameof(storage));

        public async Task<ConfigData> ReadAsync(CancellationToken cancellationToken = default)
        {
            logger.LogMethodEntry();

            var xmlFile = storage.StorageInformation;
            using var fileStream = new FileStream(xmlFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var doc = await XDocument.LoadAsync(fileStream, LoadOptions.None, cancellationToken).ConfigureAwait(false);

            CheckBasicFileStructure(doc);

            var activeProfile = GetActiveProfile(doc);
            CheckActiveProfileSection(doc, activeProfile);

            var result = new ConfigData(activeProfile, GetActiveOptions(doc), GetTargetFolder(doc, activeProfile), GetAddonUrls(doc, activeProfile));

            logger.LogMethodExit();

            return result;
        }

        private static void CheckBasicFileStructure(XDocument doc)
        {
            var root = doc.Root;

            if (root == null || root.Name != "wowcau")
            {
                throw new InvalidOperationException("Error in config file: The <wowcau> root element not exists.");
            }

            if (root.Element("general") == null)
            {
                throw new InvalidOperationException("Error in config file: The <general> section not exists.");
            }

            if (root.Element("options") == null)
            {
                throw new InvalidOperationException("Error in config file: The <options> section not exists.");
            }

            var profiles = root.Element("profiles") ?? throw new InvalidOperationException("Error in config file: The <profiles> section not exists.");
            if (!profiles.HasElements)
            {
                throw new InvalidOperationException("Error in config file: The <profiles> section not contains any profiles.");
            }
        }

        private static string GetActiveProfile(XDocument doc)
        {
            var value = doc.Root?.Element("general")?.Element("profile")?.Value?.Trim();
            var result = value ?? throw new InvalidOperationException("Error in config file: Could not determine active profile.");

            return result;
        }

        private static void CheckActiveProfileSection(XDocument doc, string activeProfile)
        {
            if (doc.Root?.Element("profiles")?.Element(activeProfile) == null)
            {
                throw new InvalidOperationException("Error in config file: The active profile, specified in <general> section, not exists in <profiles> section.");
            }
        }

        private static IEnumerable<string> GetActiveOptions(XDocument doc)
        {
            var options = doc.Root?.Element("options") ?? throw new InvalidOperationException("Error in config file: Could not determine options.");

            List<string> result = [];
            foreach (var option in options.Elements())
            {
                var value = option.Value.ToString().Trim().ToLower();
                switch (value)
                {
                    case "":
                        throw new InvalidOperationException("Error in config file: Found <options> entry with empty or whitespace value (supported values are 'true' and 'false').");
                    case "true":
                        result.Add(option.Name.ToString().Trim().ToLower());
                        break;
                    case "false":
                        // Do nothing (just not add option to result)
                        break;
                    default:
                        throw new InvalidOperationException("Error in config file: Found <options> entry with unsupported value (supported values are 'true' and 'false').");
                }
            }

            return result.AsEnumerable();
        }

        private static string GetTargetFolder(XDocument doc, string profile)
        {
            var value = doc.Root?.Element("profiles")?.Element(profile)?.Element("folder")?.Value?.Trim() ??
                throw new InvalidOperationException("Error in config file: Could not determine target folder for given profile.");

            var result = Environment.ExpandEnvironmentVariables(value);

            return result;
        }

        private static IEnumerable<string> GetAddonUrls(XDocument doc, string profile)
        {
            var addons = doc.Root?.Element("profiles")?.Element(profile)?.Element("addons") ??
                throw new InvalidOperationException("Error in config file: Could not determine addon urls for given profile.");

            var result = addons.Elements()?.Where(e => e.Name == "url")?.Select(e => e.Value.Trim().ToLower())?.Distinct() ?? [];

            return result;
        }
    }
}

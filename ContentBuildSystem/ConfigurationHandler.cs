using System;
using System.Linq;
using ContentBuildSystem.Interfaces;
using ContentBuildSystem.Project;

namespace ContentBuildSystem;

public class ConfigurationHandler
{
    private struct PartType
    {
        public string Name;
        public string Value;
        public string[] ValidValues;
    }

    private PartType[] _parts = [];

    public bool Prepare(ConfigurationDescription? cfgDesc, string? activeConfiguration, IReport? report)
    {
        if (cfgDesc == null)
            return true;

        string configuration = activeConfiguration ?? cfgDesc.Default ?? "default";
        string format = cfgDesc.Format ?? "platform";
        char separator = string.IsNullOrEmpty(cfgDesc.SplitChar) ? '-' : cfgDesc.SplitChar[0];

        string[] formatParts = format.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string[] configParts = configuration.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (formatParts.Length == 0)
        {
            report?.Error($"No format parts defined for: '{format}'");
            return false;
        }

        if (formatParts.Length != configParts.Length)
        {
            report?.Error($"Mismatch of configuration parts count, got {configParts.Length} expected {formatParts.Length}");
            return false;
        }

        bool allValid = true;
        _parts = new PartType[formatParts.Length];
        for (int i = 0; i < formatParts.Length; i++)
        {
            _parts[i].Name = formatParts[i];
            _parts[i].Value = configParts[i];

            string[]? valid = null;
            cfgDesc.Components?.TryGetValue(formatParts[i], out valid);
            _parts[i].ValidValues = valid ?? ["default"];

            if (!_parts[i].ValidValues.Contains(_parts[i].Value))
            {
                report?.Error($"Invalid configuration '{_parts[i].Value}' for '{_parts[i].Name}'");
                allValid = false;
            }
        }

        return allValid;
    }

    public bool IsApplicable(RulesetConfiguration? rulesetCfg)
    {
        if (rulesetCfg == null)
            return true;

        if (rulesetCfg.Blacklist != null)
        {
            foreach (PartType partType in _parts)
            {
                if (!rulesetCfg.Blacklist.TryGetValue(partType.Name, out string[]? blacklisted) || blacklisted == null)
                    continue;

                if (blacklisted.Contains(partType.Value))
                    return false;
            }
        }
       
        if (rulesetCfg.Whitelist != null)
        {
            foreach (PartType partType in _parts)
            {
                if (!rulesetCfg.Whitelist.TryGetValue(partType.Name, out string[]? whitelisted) || whitelisted == null)
                    continue;

                if (whitelisted.Contains(partType.Value))
                    return true;
            }

            return false;
        }

        return true;
    }
}
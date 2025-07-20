using System;

namespace GenericGameAssetsProcessors.Localization;

[Serializable]
public class LangStringsProcessorSettings
{
    public static readonly LangStringsProcessorSettings Default = new()
    {
        Delimiter = "|",
    };

    public string? Delimiter;
}

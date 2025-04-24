namespace GenericAssets.Localization;

public class LangStringsProcessorSettings
{
    public static LangStringsProcessorSettings Default = new LangStringsProcessorSettings
    {
        Delimiter = "|"
    };

    public string? Delimiter;
}
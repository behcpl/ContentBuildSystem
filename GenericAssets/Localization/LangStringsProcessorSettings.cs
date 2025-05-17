namespace GenericAssets.Localization;

public class LangStringsProcessorSettings
{
    public static LangStringsProcessorSettings Default = new()
    {
        Delimiter = "|",
    };

    public string? Delimiter;
}

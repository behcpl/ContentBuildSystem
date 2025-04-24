using System;

namespace DefaultProcessors.CopyFile;

[Serializable]
public class CopyFileProcessorSettings
{
    public static CopyFileProcessorSettings Default = new CopyFileProcessorSettings();
    
    public bool KeepExtensionCase; // false - force lowercase
    // TODO: add renaming options
    // string Format; "prefix_{0}_decor"
    // Regex ExtractGroups => variable number of capture groups that can be used for 'Format' as {1..n}
}
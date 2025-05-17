using System;

namespace DefaultProcessors.ExternalTool;

[Serializable]
public class ExternalToolSettings
{
    public string? Command;
    public string? SourceRegex;
}

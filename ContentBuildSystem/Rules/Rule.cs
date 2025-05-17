using System;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem.Rules;

public readonly struct Rule
{
    public readonly RuleHeader Header;
    public readonly DateTime LastUpdateTime;
    public readonly IItemProcessorFactory ProcessorFactory;
    public readonly object? Settings;

    public Rule(RuleHeader header, DateTime lastUpdateTime, IItemProcessorFactory processorFactory, object? settings)
    {
        Header = header;
        LastUpdateTime = lastUpdateTime;
        ProcessorFactory = processorFactory;
        Settings = settings;
    }
}

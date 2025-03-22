using System;
using System.Collections.Generic;

namespace ContentBuildSystem.Interfaces;

public readonly struct PluginDescriptor
{
    public readonly string Name;
    public readonly IItemProcessorFactory Factory;
    public readonly Type SettingsType;

    public PluginDescriptor(string name, IItemProcessorFactory factory, Type settingsType)
    {
        Name = name;
        Factory = factory;
        SettingsType = settingsType;
    }
}

public interface IPlugin
{
    public bool Initialize(IReadOnlyDictionary<string, object>? options);

    IEnumerable<PluginDescriptor> Descriptors { get; }
}
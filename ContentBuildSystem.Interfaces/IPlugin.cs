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
    public bool Initialize(IServiceRepository serviceRepository, IReadOnlyDictionary<string, object>? options, IReport? report);

    IEnumerable<PluginDescriptor> Descriptors { get; }
}
using System;
using System.Collections.Generic;
using ContentBuildSystem.Interfaces;
using DefaultProcessors.CopyFile;

namespace DefaultProcessors;

[PluginEntrypoint]
public class PluginEntrypoint : IPlugin
{
    private readonly List<PluginDescriptor> _descriptors;

    public PluginEntrypoint()
    {
        _descriptors = new List<PluginDescriptor>();
    }

    public bool Initialize(IServiceRepository serviceRepository, IReadOnlyDictionary<string, object>? options, IReport? report)
    {
        _descriptors.Add(new PluginDescriptor("copy", new CopyFileProcessorFactory(), typeof(CopyFileProcessorSettings)));

        return true;
    }

    public IEnumerable<PluginDescriptor> Descriptors => _descriptors;
}
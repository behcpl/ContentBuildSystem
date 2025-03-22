using System.Collections.Generic;
using ContentBuildSystem.Interfaces;
using DefaultProcessors.CopyFile;

namespace DefaultProcessors;

[PluginEntrypoint]
public class PluginEntrypoint : IPlugin
{
    private readonly List<PluginDescriptor> _descriptors;

    public IEnumerable<PluginDescriptor> Descriptors => _descriptors;

    public PluginEntrypoint()
    {
        _descriptors = new List<PluginDescriptor>();
    }

    public bool Initialize(IReadOnlyDictionary<string, object>? options)
    {
        _descriptors.Add(new PluginDescriptor("copy", new CopyFileProcessorFactory(), typeof(CopyFileSettings)));

        return true;
    }
}
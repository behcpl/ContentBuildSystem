using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem.CLI;

public class PluginManager
{
    private readonly string _projectPath;
    private readonly string _rootPath;
    private readonly IServiceRepository _serviceRepository;
    private readonly IReport? _report;

    public PluginManager(string projectPath, string rootPath, IServiceRepository serviceRepository, IReport? report)
    {
        _projectPath = projectPath;
        _rootPath = rootPath;
        _serviceRepository = serviceRepository;
        _report = report;
    }

    public IPlugin LoadPlugin(string pluginPath, Dictionary<string, object>? options)
    {
        string path = Path.Combine(_projectPath, pluginPath);
        if (!File.Exists(path))
            path = Path.Combine(_rootPath, pluginPath);

        if (!File.Exists(path))
            throw new NotImplementedException($"No plugin found at path: '{pluginPath}'!");

        LoadContext context = new(path, _report);
        Assembly assembly = context.LoadFromAssemblyPath(path);

        Type[] types = assembly.GetTypes();
        foreach (Type type in types)
        {
            if (!type.IsAssignableTo(typeof(IPlugin)))
                continue;

            // TODO: check for [PluginEntrypoint]
            // TODO: check for [PluginOption] for some validation?

            IPlugin plugin = (IPlugin)Activator.CreateInstance(type)!;
            bool success = plugin.Initialize(_serviceRepository, options, _report);

            // TODO: check against multiple entry points
            return plugin;
        }

        throw new NotImplementedException("No valid entry-point found!");
    }
}

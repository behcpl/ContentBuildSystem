using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem.CLI;

public class LoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public LoadContext(string pluginPath)
    {
        Console.WriteLine($"SETUP PATH {pluginPath}");
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        Console.WriteLine($"TRY LOAD {assemblyName.FullName}");

        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            Console.WriteLine($"FOUND AT {assemblyPath}");
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        Console.WriteLine($"TRY LOAD UNMANAGED {unmanagedDllName}");

        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            Console.WriteLine($"FOUND AT {libraryPath}");
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}

public class PluginManager
{
    private readonly string _pluginsPath;

    // private readonly LoadContext _loadContext;
    private readonly List<IPlugin> _plugins;

    public PluginManager(string pluginsPath)
    {
        _pluginsPath = pluginsPath;
        // _loadContext = new LoadContext(_pluginsPath);
        _plugins = new List<IPlugin>();
    }

    public IPlugin LoadPlugin(string name, Dictionary<string, object>? options)
    {
        string path = Path.Combine(_pluginsPath, name);
        LoadContext context = new LoadContext(path);
        Assembly assembly = context.LoadFromAssemblyPath(path);

        Type[] types = assembly.GetTypes();
        foreach (Type type in types)
        {
            if (!type.IsAssignableTo(typeof(IPlugin)))
                continue;

            // TODO: check for [PluginEntrypoint]
            // TODO: check for [PluginOption] for some validation?

            IPlugin plugin = (IPlugin)Activator.CreateInstance(type)!;
            plugin.Initialize(options);
            _plugins.Add(plugin);

            // TODO: check against multiple entry points
            return plugin;
        }

        throw new NotImplementedException("No valid entry-point found!");
    }
}
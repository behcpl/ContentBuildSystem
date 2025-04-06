using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem.CLI;

public class LoadContext : AssemblyLoadContext
{
    private readonly IReport? _report;
    private readonly AssemblyDependencyResolver _resolver;

    private readonly Dictionary<string, Assembly> _assemblies;
    private readonly Dictionary<string, IntPtr> _native;

    public LoadContext(string pluginPath, IReport? report)
    {
        _report = report;
        _resolver = new AssemblyDependencyResolver(pluginPath);

        _assemblies = new Dictionary<string, Assembly>();
        _native = new Dictionary<string, IntPtr>();
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            if (_assemblies.TryGetValue(assemblyPath, out Assembly? loaded))
                return loaded;

            _report?.Info($"MANAGED DLL: {assemblyPath}");
            Assembly assembly = LoadFromAssemblyPath(assemblyPath);
            _assemblies.Add(assemblyPath, assembly);

            return assembly;
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            if (_native.TryGetValue(libraryPath, out IntPtr loaded))
                return loaded;

            _report?.Info($"NATIVE DLL: {libraryPath}");
            IntPtr handle = LoadUnmanagedDllFromPath(libraryPath);
            _native.Add(libraryPath, handle);

            return handle;
        }

        return IntPtr.Zero;
    }
}
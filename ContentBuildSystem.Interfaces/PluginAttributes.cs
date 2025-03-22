using System;

namespace ContentBuildSystem.Interfaces;

[AttributeUsage(AttributeTargets.Class)]
public class PluginEntrypointAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginOptionAttribute : Attribute
{
    public readonly string Name;
    public readonly Type Type;

    public PluginOptionAttribute(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}
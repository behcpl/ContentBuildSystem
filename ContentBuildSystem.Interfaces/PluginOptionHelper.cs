using System.Collections.Generic;

namespace ContentBuildSystem.Interfaces;

public static class PluginOptionHelper
{
    public static T GetValue<T>(this IReadOnlyDictionary<string, object>? options, string name, T defaultValue)
    {
        return options != null && options.TryGetValue(name, out object? oVal) && oVal is T tVal ? tVal : defaultValue;
    }
}
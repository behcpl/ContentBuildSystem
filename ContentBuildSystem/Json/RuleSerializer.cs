using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using ContentBuildSystem.Rules;

namespace ContentBuildSystem.Json;

public class RuleSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        AllowTrailingCommas = true,
        IncludeFields = true
    };

    public RuleHeader? GetHeader(string text)
    {
        return JsonSerializer.Deserialize<RuleHeader>(text, _options);
    }

    public Dictionary<string, object> GetSettings(string text, Type settingsType)
    {
        Type descType = typeof(RuleSettings<>).MakeGenericType(settingsType);
        object? result = JsonSerializer.Deserialize(text, descType, _options);

        return (Dictionary<string, object>)GetType().GetMethod(nameof(Convert), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(settingsType).Invoke(null, new object[] { result! })!;
    }

    private static Dictionary<string, object> Convert<T>(object parameter) where T : class
    {
        RuleSettings<T> ruleSettings = (RuleSettings<T>)parameter;
        Dictionary<string, object> newDict = new Dictionary<string, object>();

        foreach (var kv in ruleSettings.Settings)
        {
            newDict.Add(kv.Key, kv.Value);
        }

        return newDict;
    }

    private class RuleSettings<T> where T : class
    {
        public Dictionary<string, T>? Settings;
    }
}
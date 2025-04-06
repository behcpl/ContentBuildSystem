using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public object? GetRawSettings(string text, Type settingsType)
    {
        Type descType = typeof(RuleSettings<>).MakeGenericType(settingsType);
        IRuleSettings? result = JsonSerializer.Deserialize(text, descType, _options) as IRuleSettings;

        return result?.RawSettings;
    }

    private interface IRuleSettings
    {
        object? RawSettings { get; }
    }

    [Serializable]
    private class RuleSettings<T> : IRuleSettings where T : class
    {
        [JsonIgnore] public object? RawSettings => Settings;

        // Deserialized only
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public T? Settings;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }
}
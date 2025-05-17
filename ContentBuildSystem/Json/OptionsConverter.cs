using System.Collections.Generic;
using System.Text.Json;

namespace ContentBuildSystem.Json;

internal static class OptionsConverter
{
    public static Dictionary<string, object>? Map(Dictionary<string, object>? source)
    {
        if (source == null)
            return null;

        Dictionary<string, object> mapped = new();
        foreach (KeyValuePair<string, object> pair in source)
        {
            JsonElement e = (JsonElement)pair.Value;

            switch (e.ValueKind)
            {
                case JsonValueKind.Undefined:
                    break;

                case JsonValueKind.Object:
                    break;

                case JsonValueKind.Array:
                    break;

                case JsonValueKind.String:
                    mapped.Add(pair.Key, e.GetString()!);
                    break;

                case JsonValueKind.Number:
                    if (e.TryGetInt32(out int iVal))
                        mapped.Add(pair.Key, iVal);
                    else if (e.TryGetSingle(out float fVal))
                        mapped.Add(pair.Key, fVal);
                    break;

                case JsonValueKind.True:
                    mapped.Add(pair.Key, true);
                    break;

                case JsonValueKind.False:
                    mapped.Add(pair.Key, false);
                    break;

                case JsonValueKind.Null:
                    break;
            }
        }

        return mapped;
    }
}

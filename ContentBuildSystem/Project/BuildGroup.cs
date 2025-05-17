using System.Collections.Generic;
using ContentBuildSystem.Rules;

namespace ContentBuildSystem.Project;

public class BuildGroup
{
    public readonly struct ItemType
    {
        public readonly string Path;
        public readonly Rule Rule;

        public ItemType(string path, Rule rule)
        {
            Path = path;
            Rule = rule;
        }
    }

    public readonly List<ItemType> Items = new();
}

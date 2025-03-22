using System.Collections.Generic;
using JetBrains.Annotations;

namespace GenericAssets.Legacy.Atlases;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SpriteAtlasMeta //: AssetMeta
{
    public List<string>? Files;
    public List<string>? Folders;
    
    public int Spacing;
    public bool Padding;
    public bool Compress;
}
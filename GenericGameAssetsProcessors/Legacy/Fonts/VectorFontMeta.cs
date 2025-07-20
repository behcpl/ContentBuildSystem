using JetBrains.Annotations;

namespace GenericGameAssetsProcessors.Legacy.Fonts;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class VectorFontMeta //: AssetMeta
{
    public int FontSize;
    public int? SdfRange;
    public string? Characters;
}

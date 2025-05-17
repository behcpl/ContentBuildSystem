using System.Collections.Generic;
using ContentBuildSystem.Interfaces;
using GenericAssets.Legacy.Atlases;
using GenericAssets.Legacy.Fonts;
using GenericAssets.Legacy.Textures;
using GenericAssets.Localization;
using GenericAssets.Shader;

namespace GenericAssets;

[PluginEntrypoint]
[PluginOption("DebugOutput", typeof(bool))]
public class PluginEntrypoint : IPlugin
{
    private readonly List<PluginDescriptor> _descriptors;

    public PluginEntrypoint()
    {
        _descriptors = new List<PluginDescriptor>();
    }

    public bool Initialize(IServiceRepository serviceRepository, IReadOnlyDictionary<string, object>? options, IReport? report)
    {
        bool debugOutput = options.GetValue("DebugOutput", false);

        // _descriptors.Add(new PluginDescriptor("texture", new ImageProcessorFactory(), typeof(object)));
        _descriptors.Add(new PluginDescriptor("csv2json", new LangStringsProcessorFactory(), typeof(object)));
        _descriptors.Add(new PluginDescriptor("shaders", new ShaderGroupProcessorFactory(), typeof(ShaderGroupProcessorSettings)));
        _descriptors.Add(new PluginDescriptor("bmfont", new BitmapFontProcessorFactory(), typeof(BitmapFontProcessorSettings)));
        _descriptors.Add(new PluginDescriptor("fontgen", new SdfFontProcessorFactory(debugOutput), typeof(SdfFontProcessorSettings)));
        _descriptors.Add(new PluginDescriptor("texture", new TextureProcessorFactory(), typeof(TextureProcessorSettings)));
        _descriptors.Add(new PluginDescriptor("atlas", new SpriteAtlasProcessorFactory(debugOutput), typeof(SpriteAtlasProcessorSettings)));

        return true;
    }

    public IEnumerable<PluginDescriptor> Descriptors => _descriptors;
}

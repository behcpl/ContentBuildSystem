using System.Collections.Generic;
using ContentBuildSystem.Interfaces;
using GenericGameAssetsProcessors.ImageImporter;
using GenericGameAssetsProcessors.Legacy.Atlases;
using GenericGameAssetsProcessors.Legacy.Fonts;
using GenericGameAssetsProcessors.Legacy.Textures;
using GenericGameAssetsProcessors.Localization;
using GenericGameAssetsProcessors.Services;
using GenericGameAssetsProcessors.Shader;

namespace GenericGameAssetsProcessors;

[PluginEntrypoint]
[PluginOption(_DEBUG_OUTPUT, typeof(bool))]
public class PluginEntrypoint : IPlugin
{
    private const string _DEBUG_OUTPUT = "DebugOutput";

    private readonly List<PluginDescriptor> _descriptors;

    public PluginEntrypoint()
    {
        _descriptors = new List<PluginDescriptor>();
    }

    public bool Initialize(IServiceRepository serviceRepository, IReadOnlyDictionary<string, object>? options, IReport? report)
    {
        bool debugOutput = options.GetValue(_DEBUG_OUTPUT, false);

        IDebugImage debugImage = new DebugImage();
        IImporterGroup<ImageData> imageImporterGroup = new ImageImporterGroup();
        imageImporterGroup.AddImporter(new StbImageImporter(), StbImageImporter.SupportedExtensions);

        serviceRepository.Add(debugImage);
        serviceRepository.Add(imageImporterGroup);

        // _descriptors.Add(new PluginDescriptor("texture", new ImageProcessorFactory(), typeof(object)));
        _descriptors.Add(new PluginDescriptor("csv2json", new LangStringsProcessorFactory(), typeof(object)));
        _descriptors.Add(new PluginDescriptor("shaders", new ShaderGroupProcessorFactory(), typeof(ShaderGroupProcessorSettings)));
        _descriptors.Add(new PluginDescriptor("bmfont", new BitmapFontProcessorFactory(), typeof(BitmapFontProcessorSettings)));
        _descriptors.Add(new PluginDescriptor("fontgen", new SdfFontProcessorFactory(debugOutput), typeof(SdfFontProcessorSettings)));
        _descriptors.Add(new PluginDescriptor("texture", new TextureProcessorFactory(), typeof(TextureProcessorSettings)));
        _descriptors.Add(new PluginDescriptor("atlas", new SpriteAtlasProcessorFactory(debugOutput), typeof(SpriteAtlasProcessorSettings)));

        _descriptors.Add(new PluginDescriptor("texture2", new Texture.TextureProcessorFactory(imageImporterGroup, debugImage, debugOutput), typeof(Texture.TextureProcessorSettings)));


        return true;
    }

    public IEnumerable<PluginDescriptor> Descriptors => _descriptors;
}

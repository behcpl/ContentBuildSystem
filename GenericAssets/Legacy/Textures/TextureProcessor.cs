using System.IO;
using ContentBuildSystem.Interfaces;

namespace GenericAssets.Legacy.Textures;

public class TextureProcessor : IItemProcessor
{
    private readonly IProcessorContext _context;
    private readonly TextureProcessorSettings _settings;
    private readonly string _outputPath;

    public TextureProcessor(IProcessorContext context, TextureProcessorSettings? settings)
    {
        _context = context;
        _settings = settings ?? new TextureProcessorSettings();

        _outputPath = TextureProcessorFactory.GetDefaultOutputPath(context);
    }

    public bool Process(IReport? report)
    {
        TextureImporter textureImporter = new();
        TextureBinarySerializer serializer = new();

        Directory.CreateDirectory(Path.Combine(_context.OutputPath, _context.ItemRelativePath));

        TextureSource texture = textureImporter.Import(_context.ItemPath, _settings, report);
        serializer.Serialize(texture, _outputPath);

        return true;
    }
}

public class TextureProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => true;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings) => GetDefaultOutputPath(context);

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new TextureProcessor(context, settings as TextureProcessorSettings);
    }

    public static string GetDefaultOutputPath(IProcessorContext context) => Path.GetFullPath(Path.Combine(context.OutputPath, context.ItemRelativePath, $"{context.ItemName}.bctex"));
}

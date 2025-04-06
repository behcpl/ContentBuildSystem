using System;
using System.IO;
using ContentBuildSystem.Interfaces;

namespace GenericAssets.Legacy.Textures;

[Serializable]
public class TextureProcessorSettings
{
    public bool PremultipliedAlpha;
    public bool LinearSpace;

    // transform
    public bool PremultiplyAlpha;
    public bool Compress;
    public bool AddFrame;
}

public class TextureProcessor : IItemProcessor
{
    private readonly IProcessorContext _context;
    private readonly TextureProcessorSettings _settings;
    private readonly string _outputPath;

    public TextureProcessor(IProcessorContext context, TextureProcessorSettings? settings)
    {
        _context = context;
        _settings = settings ?? new TextureProcessorSettings();

        _outputPath = Path.GetFullPath(Path.Combine(_context.OutputPath, _context.ItemRelativePath, $"{_context.ItemName}.bctex"));
    }

    public bool Process(IReport? report)
    {
        TextureImporter textureImporter = new TextureImporter(true);
        TextureBinarySerializer serializer = new TextureBinarySerializer();

        Directory.CreateDirectory(Path.Combine(_context.OutputPath, _context.ItemRelativePath));

        TextureSource texture = textureImporter.Import(_context.ItemPath, _settings, report);
        serializer.Serialize(texture, _outputPath);

        return true;
    }

    public string[] GetOutputPaths()
    {
        return [_outputPath];
    }

    public string[] GetDependencies()
    {
        return [];
    }
}

public class TextureProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => true;

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new TextureProcessor(context, settings as TextureProcessorSettings);
    }
}
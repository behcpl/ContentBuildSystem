using System.Collections.Generic;
using System.IO;
using ContentBuildSystem.Interfaces;
using GenericAssets.Legacy.Textures;

namespace GenericAssets.Legacy.Fonts;

public class BitmapFontProcessorSettings
{
    // ?
}

public class BitmapFontProcessor : IItemProcessor
{
    private readonly IProcessorContext _context;

    private string _outputPath;
    private readonly List<string> _inputFiles;

    public BitmapFontProcessor(IProcessorContext context, BitmapFontProcessorSettings? settings)
    {
        _context = context;

        _outputPath = string.Empty;
        _inputFiles = new List<string>();
    }

    public bool Process(IReport? report)
    {
        TextureImporter texImporter = new TextureImporter();
        BitmapFontImporter bmfImporter = new BitmapFontImporter(texImporter, report);
        FontBinarySerializer serializer = new FontBinarySerializer();

        FontSource font = bmfImporter.Import(_context.ItemPath, _inputFiles);

        Directory.CreateDirectory(Path.Combine(_context.OutputPath, _context.ItemRelativePath));
        _inputFiles.Add(_context.ItemPath);
        _outputPath = Path.GetFullPath(Path.Combine(_context.OutputPath, _context.ItemRelativePath, $"{_context.ItemName}.bcfnt"));
        serializer.Serialize(font, _outputPath);

        return true;
    }

    public string[] GetOutputPaths()
    {
        return [_outputPath];
    }

    public string[] GetDependencies()
    {
        return _inputFiles.ToArray();
    }
}

public class BitmapFontProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => false;

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new BitmapFontProcessor(context, settings as BitmapFontProcessorSettings);
    }
}
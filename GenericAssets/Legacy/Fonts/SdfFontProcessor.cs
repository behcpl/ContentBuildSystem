using System;
using System.Collections.Generic;
using System.IO;
using ContentBuildSystem.Interfaces;
using GenericAssets.Legacy.Textures;

namespace GenericAssets.Legacy.Fonts;

public class SdfFontProcessorSettings
{
    // ?
}

public class SdfFontProcessor : IItemProcessor
{
    private readonly IProcessorContext _context;
    private readonly bool _debugOutput;

    private string _outputPath;
    private readonly List<string> _inputFiles;

    public SdfFontProcessor(IProcessorContext context, SdfFontProcessorSettings? settings, bool debugOutput)
    {
        _context = context;
        _debugOutput = debugOutput;

        _outputPath = string.Empty;
        _inputFiles = new List<string>();
    }

    public bool Process(IReport? report)
    {
        SdfFontGenerator generator = new SdfFontGenerator();
        FontSource font = generator.Import(_context.ItemPath);
        FontBinarySerializer serializer = new FontBinarySerializer();

        Directory.CreateDirectory(Path.Combine(_context.OutputPath, _context.ItemRelativePath));
        _inputFiles.Add(_context.ItemPath);
        _outputPath = Path.GetFullPath(Path.Combine(_context.OutputPath, _context.ItemRelativePath, $"{_context.ItemName}.bcfnt"));
        _context.RegisterOutputArtifact(_outputPath);
        serializer.Serialize(font, _outputPath);

        if (_debugOutput)
        {
            TextureDebugOutput dbgOut = new TextureDebugOutput(_context.TempPath);
            dbgOut.DebugOut(_context.ItemName, font.Texture!);
        }

        return true;
    }
}

public class SdfFontProcessorFactory : IItemProcessorFactory
{
    private readonly bool _debugOutput;

    public SdfFontProcessorFactory(bool debugOutput)
    {
        _debugOutput = debugOutput;
    }

    public bool SimpleProcessor => false;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        throw new NotSupportedException();
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new SdfFontProcessor(context, settings as SdfFontProcessorSettings, _debugOutput);
    }
}
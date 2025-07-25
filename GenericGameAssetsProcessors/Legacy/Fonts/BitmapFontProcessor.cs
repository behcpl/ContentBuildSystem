﻿using System;
using System.IO;
using ContentBuildSystem.Interfaces;
using GenericGameAssetsProcessors.Legacy.Textures;

namespace GenericGameAssetsProcessors.Legacy.Fonts;

public class BitmapFontProcessorSettings
{
    // ?
}

public class BitmapFontProcessor : IItemProcessor
{
    private readonly IProcessorContext _context;

    private string _outputPath;

    public BitmapFontProcessor(IProcessorContext context, BitmapFontProcessorSettings? settings)
    {
        _context = context;

        _outputPath = string.Empty;
    }

    public bool Process(IReport? report)
    {
        TextureImporter texImporter = new();
        BitmapFontImporter bmfImporter = new(texImporter, report);
        FontBinarySerializer serializer = new();

        FontSource font = bmfImporter.Import(_context.ItemPath, _context);

        Directory.CreateDirectory(Path.Combine(_context.OutputPath, _context.ItemRelativePath));
        _outputPath = Path.GetFullPath(Path.Combine(_context.OutputPath, _context.ItemRelativePath, $"{_context.ItemName}.bcfnt"));
        serializer.Serialize(font, _outputPath);
        _context.RegisterOutputArtifact(_outputPath);

        return true;
    }
}

public class BitmapFontProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => false;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        throw new NotSupportedException();
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new BitmapFontProcessor(context, settings as BitmapFontProcessorSettings);
    }
}

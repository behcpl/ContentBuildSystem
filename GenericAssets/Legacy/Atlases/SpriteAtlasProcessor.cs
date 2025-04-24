using System.Collections.Generic;
using System.IO;
using ContentBuildSystem.Interfaces;
using GenericAssets.Legacy.Textures;

namespace GenericAssets.Legacy.Atlases;

public class SpriteAtlasProcessorSettings
{
    // ?
}

public class SpriteAtlasProcessor : IItemProcessor
{
    private readonly IProcessorContext _context;

    public SpriteAtlasProcessor(IProcessorContext context, SpriteAtlasProcessorSettings? settings, bool debugOutput)
    {
        _context = context;
    }

    public bool Process(IReport? report)
    {
        TextureImporter textureImporter = new TextureImporter(false);
        SpriteAtlasBuilder spriteAtlasBuilder = new SpriteAtlasBuilder(textureImporter, report);
        SpriteAtlasBinarySerializer serializer = new SpriteAtlasBinarySerializer();

        TextureProcessorSettings textureProcessorSettings = new TextureProcessorSettings { Compress = false };

        string outPath = Path.Combine(_context.OutputPath, _context.ItemRelativePath, $"{_context.ItemName}.bcatl");
        _context.RegisterOutputArtifact(outPath);

        SpriteAtlasSource atlas = spriteAtlasBuilder.Import(_context.ItemPath, _context, textureProcessorSettings);
        serializer.Serialize(atlas, outPath);

        TextureDebugOutput dbgOut = new TextureDebugOutput(_context.TempPath);
        dbgOut.DebugOut(_context.ItemName, atlas.Texture!);

        return true;
    }
}

public class SpriteAtlasProcessorFactory : IItemProcessorFactory
{
    private readonly bool _debugOutput;

    public SpriteAtlasProcessorFactory(bool debugOutput)
    {
        _debugOutput = debugOutput;
    }

    public bool SimpleProcessor => false;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        throw new System.NotSupportedException();
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new SpriteAtlasProcessor(context, settings as SpriteAtlasProcessorSettings, _debugOutput);
    }
}
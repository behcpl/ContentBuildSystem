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

    private readonly List<string> _outputs;
    private readonly List<string> _inputFiles;

    public SpriteAtlasProcessor(IProcessorContext context, SpriteAtlasProcessorSettings? settings, bool debugOutput)
    {
        _context = context;

        _outputs = new List<string>();
        _inputFiles = new List<string>();
    }

    public bool Process(IReport? report)
    {
        TextureImporter textureImporter = new TextureImporter(false);
        SpriteAtlasBuilder spriteAtlasBuilder = new SpriteAtlasBuilder(textureImporter);
        SpriteAtlasBinarySerializer serializer = new SpriteAtlasBinarySerializer();

        TextureProcessorSettings textureProcessorSettings = new TextureProcessorSettings { Compress = false };

        string outPath = Path.Combine(_context.OutputPath, _context.GroupPath, $"{_context.ItemName}.bcatl");
        _outputs.Add(outPath);

        SpriteAtlasSource atlas = spriteAtlasBuilder.Import(_context.ItemPath, _inputFiles, textureProcessorSettings);
        serializer.Serialize(atlas, outPath);

        TextureDebugOutput dbgOut = new TextureDebugOutput(_context.TempPath);
        dbgOut.DebugOut(_context.ItemName, atlas.Texture!);

        return true;
    }

    public string[] GetOutputPaths()
    {
        return _outputs.ToArray();
    }

    public string[] GetDependencies()
    {
        return _inputFiles.ToArray();
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

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new SpriteAtlasProcessor(context, settings as SpriteAtlasProcessorSettings, _debugOutput);
    }
}
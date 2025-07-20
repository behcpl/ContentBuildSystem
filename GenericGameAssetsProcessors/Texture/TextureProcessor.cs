using System;
using System.IO;
using ContentBuildSystem.Interfaces;
using GenericGameAssets;
using GenericGameAssets.Serialization;
using GenericGameAssetsProcessors.ImageImporter;
using GenericGameAssetsProcessors.Legacy.Textures;
using GenericGameAssetsProcessors.Services;
using TextureFormat = GenericGameAssets.TextureFormat;

namespace GenericGameAssetsProcessors.Texture;

public class TextureProcessor : IItemProcessor
{
    private readonly IImporterGroup<ImageData> _imageImporter;
    private readonly IDebugImage _debugImage;
    private readonly bool _debugOutput;
    private readonly IProcessorContext _context;
    private readonly TextureProcessorSettings _settings;

    public TextureProcessor(IImporterGroup<ImageData> imageImporter, IDebugImage debugImage, bool debugOutput, IProcessorContext context, TextureProcessorSettings settings)
    {
        _imageImporter = imageImporter;
        _debugImage = debugImage;
        _debugOutput = debugOutput;
        _context = context;
        _settings = settings;
    }

    public bool Process(IReport? report)
    {
        if (!_imageImporter.TryImport(_context.ItemPath, out ImageData? imageData, report))
            return false;

        // TODO: process data

        // TODO: store data

        // TODO: optional, debug out data

        TextureBuffer buffer = new TextureBuffer
        {
            Data = imageData.ConvertToRGBA8888(),
        };

        TextureAsset textureAsset = new TextureAsset
        {
            Type = TextureAssetType.TEXTURE_2D,
            Format = TextureFormat.MakeUncompressed(TextureFormat.CHANNEL_RGBA, TextureFormat.TYPE_1B_UNORM_GAMMA),
            Width = imageData.Width,
            Height = imageData.Height,
            Count = 1,
            MipMaps = 1,
            Buffers = new TextureBuffer[1],
        };
        textureAsset.Buffers[0] = buffer;

        string outputPath = TextureProcessorFactory.GetDefaultOutputPath(_context);
        BinaryWriter writer = new BinaryWriter(File.Open(outputPath, FileMode.Create));
        TextureAssetSerializer serializer = new TextureAssetSerializer();
        serializer.Write(writer, textureAsset);
        return true;
    }
}

public class TextureProcessorFactory : IItemProcessorFactory
{
    private readonly IImporterGroup<ImageData> _imageImporter;
    private readonly IDebugImage _debugImage;
    private readonly bool _debugOutput;

    public TextureProcessorFactory(IImporterGroup<ImageData> imageImporter, IDebugImage debugImage, bool debugOutput)
    {
        _imageImporter = imageImporter;
        _debugImage = debugImage;
        _debugOutput = debugOutput;
        throw new NotImplementedException();
    }

    public bool SimpleProcessor => true;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        return GetDefaultOutputPath(context);
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new TextureProcessor(_imageImporter, _debugImage, _debugOutput, context, settings as TextureProcessorSettings ?? TextureProcessorSettings.Default);
    }

    public static string GetDefaultOutputPath(IProcessorContext context) => Path.GetFullPath(Path.Combine(context.OutputPath, context.ItemRelativePath, $"{context.ItemName}.bctex"));
}

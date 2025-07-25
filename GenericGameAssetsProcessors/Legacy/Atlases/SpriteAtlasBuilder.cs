﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ContentBuildSystem.Interfaces;
using GenericGameAssetsProcessors.Legacy.Textures;
using StbRectPackSharp;

// using YamlDotNet.Serialization;
// using YamlDotNet.Serialization.NamingConventions;

namespace GenericGameAssetsProcessors.Legacy.Atlases;

public class SpriteAtlasTexture
{
    public string? Name;
    public TextureSource? Texture;
    public Margin Placement;
}

public class SpriteAtlasTextureComparer : IComparer<SpriteAtlasTexture>
{
    public int Compare(SpriteAtlasTexture? x, SpriteAtlasTexture? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;

        int ret = y.Texture!.Width.CompareTo(x.Texture!.Width);
        if (ret != 0)
            return ret;

        return y.Texture!.Height.CompareTo(x.Texture!.Height);
    }
}

public class SpriteAtlasBuilder
{
    private readonly TextureImporter _textureImporter;
    private readonly IReport? _report;

    private readonly JsonSerializerOptions _options;

    public SpriteAtlasBuilder(TextureImporter textureImporter, IReport? report)
    {
        _textureImporter = textureImporter;
        _report = report;
        _options = new JsonSerializerOptions { IncludeFields = true, AllowTrailingCommas = true };
    }

    public SpriteAtlasSource Import(string path, IProcessorContext context, TextureProcessorSettings textureProcessorSettings)
    {
        string directoryPath = Path.GetDirectoryName(path)!;

        SpriteAtlasMeta meta = JsonSerializer.Deserialize<SpriteAtlasMeta>(File.ReadAllText(path), _options)!;

        Dictionary<string, SpriteExtras> spriteExtras = meta.SpriteExtras ?? new Dictionary<string, SpriteExtras>();

        List<SpriteAtlasTexture> textures = new();
        LoadFiles(directoryPath, textures, meta.Files, context, textureProcessorSettings, spriteExtras);
        LoadFolders(directoryPath, textures, meta.Folders, context, textureProcessorSettings, spriteExtras);

        if (textures.Count == 0)
            throw new Exception($"Empty atlas: {path}");

        int maxWidth = 0;
        int totalArea = 0;
        foreach (SpriteAtlasTexture atlasTexture in textures)
        {
            maxWidth = Math.Max(maxWidth, atlasTexture.Texture!.Width);
            totalArea += atlasTexture.Texture!.Width * atlasTexture.Texture!.Height;
        }

        int widthPow2 = Math.Max(maxWidth, (int)Math.Sqrt(totalArea));
        widthPow2 |= widthPow2 >> 1;
        widthPow2 |= widthPow2 >> 2;
        widthPow2 |= widthPow2 >> 4;
        widthPow2 |= widthPow2 >> 8;
        widthPow2 |= widthPow2 >> 16;
        widthPow2++;

        int heightPow2 = widthPow2 * 4;
        Packer packer = new(widthPow2 - meta.Spacing, heightPow2 - meta.Spacing);

        textures.Sort(new SpriteAtlasTextureComparer());

        foreach (SpriteAtlasTexture atlasTexture in textures)
        {
            packer.PackRect(atlasTexture.Texture!.Width + meta.Spacing, atlasTexture.Texture!.Height + meta.Spacing, atlasTexture);
        }

        int usedHeight = 0;
        foreach (PackerRectangle packRectangle in packer.PackRectangles)
        {
            usedHeight = Math.Max(usedHeight, packRectangle.Y + packRectangle.Height + meta.Spacing);
        }

        usedHeight = (usedHeight + 3) & ~0x3;

        byte[] data = new byte[widthPow2 * usedHeight * 4];

        SpriteAtlasSource spriteAtlasSource = new();
        spriteAtlasSource.Sprites = new List<SpriteSource>(textures.Count);

        foreach (PackerRectangle packRectangle in packer.PackRectangles)
        {
            SpriteAtlasTexture atlasTexture = (SpriteAtlasTexture)packRectangle.Data;
            TextureSource texture = atlasTexture.Texture!;

            atlasTexture.Placement = new Margin
            {
                Left = meta.Spacing + packRectangle.X,
                Top = meta.Spacing + packRectangle.Y,
                Right = meta.Spacing + packRectangle.X + atlasTexture.Texture!.Width,
                Bottom = meta.Spacing + packRectangle.Y + atlasTexture.Texture!.Height,
            };

            spriteAtlasSource.Sprites.Add(
                new SpriteSource
                {
                    Placement = Margin.Offset(atlasTexture.Placement, atlasTexture.Texture!.SourceOffset),
                    Margin = atlasTexture.Texture!.Margin,
                    Pivot = atlasTexture.Texture!.Pivot,
                    Name = atlasTexture.Name,
                });

            for (int y = 0; y < texture.Height; y++)
            {
                Array.Copy(texture.Data!, y * texture.Width * 4, data, ((y + packRectangle.Y + meta.Spacing) * widthPow2 + packRectangle.X + meta.Spacing) * 4, texture.Width * 4);
            }
        }

        spriteAtlasSource.Texture = new TextureSource
        {
            Width = widthPow2,
            Height = usedHeight,
            AlphaMode = AlphaMode.PREMULTIPLIED,
            Data = data,
            Format = TextureFormat.RGBA8888,
            IsLinear = false,
        };

        packer.Dispose();
        return spriteAtlasSource;
    }

    private void LoadFiles(string directoryPath, List<SpriteAtlasTexture> textures, List<string>? metaFiles, IProcessorContext context, TextureProcessorSettings textureProcessorSettings, Dictionary<string, SpriteExtras> extras)
    {
        if (metaFiles == null || metaFiles.Count == 0)
            return;

        foreach (string file in metaFiles)
        {
            try
            {
                string nameOnly = Path.GetFileNameWithoutExtension(file);
                extras.TryGetValue(nameOnly, out SpriteExtras? spriteExtras);
                spriteExtras ??= new SpriteExtras();

                TextureSource textureSource = _textureImporter.Import(Path.Combine(directoryPath, file), textureProcessorSettings, _report);
                textureSource.Margin = spriteExtras.Margin;
                textureSource.Pivot = spriteExtras.Pivot;
                textures.Add(new SpriteAtlasTexture { Texture = textureSource, Name = Path.GetFileNameWithoutExtension(file) });
                context.RegisterSourceDependency(Path.GetFullPath(Path.Combine(directoryPath, file)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private void LoadFolders(string directoryPath, List<SpriteAtlasTexture> textures, List<string>? metaFolders, IProcessorContext context, TextureProcessorSettings textureProcessorSettings, Dictionary<string, SpriteExtras> extras)
    {
        if (metaFolders == null || metaFolders.Count == 0)
            return;

        // TODO: get this from importer
        string[] extensions = [ "png" ];

        foreach (string folder in metaFolders)
        {
            string folderPath = Path.Combine(directoryPath, folder);

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"NOP: {folderPath}");
                continue;
            }

            context.RegisterSourceFolderDependency(folderPath, false, extensions);

            string[] filePaths = DirectoryHelper.EnumerateFiles(folderPath, false, extensions);
            foreach (string filePath in filePaths)
            {
                try
                {
                    string nameOnly = Path.GetFileNameWithoutExtension(filePath);
                    extras.TryGetValue(nameOnly, out SpriteExtras? spriteExtras);
                    spriteExtras ??= new SpriteExtras();

                    TextureSource textureSource = _textureImporter.Import(filePath, textureProcessorSettings, _report);
                    textureSource.Margin = spriteExtras.Margin;
                    textureSource.Pivot = spriteExtras.Pivot;
                    textures.Add(new SpriteAtlasTexture { Texture = textureSource, Name = Path.GetFileNameWithoutExtension(filePath) });
                }
                catch (Exception e)
                {
                    _report?.Exception(e);
                }
            }
        }
    }
}

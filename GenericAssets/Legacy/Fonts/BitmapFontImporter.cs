using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GenericAssets.Legacy.Textures;

namespace GenericAssets.Legacy.Fonts;

public class BitmapFontImporter
{
    private readonly TextureImporter _textureImporter;

    public BitmapFontImporter(TextureImporter textureImporter)
    {
        _textureImporter = textureImporter;
    }

    public FontSource Import(string path, List<string> inputFiles)
    {
        FontSource fontSource = new FontSource();
        ParseBmf(fontSource, path, out string pageName);

        string pageTexturePath = Path.Combine(Path.GetDirectoryName(path)!, pageName);

        inputFiles.Add(Path.GetFullPath(pageTexturePath));

        TextureProcessorSettings settings = new TextureProcessorSettings { LinearSpace = true };

        fontSource.Texture = _textureImporter.Import(pageTexturePath, settings);

        return fontSource;
    }

    // info face="Consolas" size=42 bold=0 italic=0 charset="" unicode=1 stretchH=100 smooth=1 aa=1 padding=0,0,0,0 spacing=1,1 outline=2
    // common lineHeight=42 base=33 scaleW=256 scaleH=256 pages=1 packed=0 alphaChnl=2 redChnl=0 greenChnl=0 blueChnl=0
    // page id=0 file="debug_font_0.png"
    // chars count=101
    // char id=33   x=101   y=67    width=10    height=29    xoffset=5     yoffset=6     xadvance=20    page=0  chnl=15
    // kernings count=91
    // kerning first=32  second=65  amount=-2  
    private static void ParseBmf(FontSource font, string path, out string pageName)
    {
        font.Glyphs = new List<FontGlyphSource>();
        font.KerningTable = new List<FontKerningSource>();
        pageName = string.Empty;

        string[] lines = File.ReadAllLines(path, Encoding.ASCII);
        foreach (string line in lines)
        {
            //TODO: this will break "page" line if font name contains space char
            string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
                continue;

            switch (tokens[0])
            {
                case "info":
                    break;
                case "common":
                    ParseCommon(font, tokens);
                    break;
                case "page":
                    pageName = ParsePage(tokens);
                    break;
                case "chars":
                    break;
                case "char":
                    ParseChar(font, tokens);
                    break;
                case "kernings":
                    break;
                case "kerning":
                    ParseKerning(font, tokens);
                    break;
            }
        }

        font.EmWidth = font.BaseOffset;
        font.SpaceWidth = font.EmWidth / 4;

        FontGlyphSource? spaceGlyph = font.Glyphs.Find(g => g.Id == 32);
        if (spaceGlyph != null)
        {
            font.SpaceWidth = spaceGlyph.XAdvance;
        }
    }

    private static void ParseCommon(FontSource font, IReadOnlyList<string> tokens)
    {
        for (int index = 1; index < tokens.Count; index++)
        {
            string[] pair = tokens[index].Split('=', StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
                continue;

            switch (pair[0])
            {
                case "lineHeight":
                    font.LineHeight = int.Parse(pair[1]);
                    break;
                case "base":
                    font.BaseOffset = int.Parse(pair[1]);
                    break;
                // case "scaleW":
                //     _scaleW = int.Parse(pair[1]);
                //     break;
                // case "scaleH":
                //     _scaleH = int.Parse(pair[1]);
                //     break;
            }
        }
    }

    private static string ParsePage(IReadOnlyList<string> tokens)
    {
        for (int index = 1; index < tokens.Count; index++)
        {
            string[] pair = tokens[index].Split('=', StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
                continue;

            switch (pair[0])
            {
                case "id":
                    if (int.Parse(pair[1]) > 0)
                        throw new Exception("Multiple pages are not supported!");
                    break;

                case "file":
                    //TODO: this will break when space is inside anyway
                    return pair[1].Substring(1, pair[1].Length - 2);
            }
        }

        throw new Exception("Page not found!");
    }


    private static void ParseChar(FontSource font, IReadOnlyList<string> tokens)
    {
        FontGlyphSource glyph = new FontGlyphSource();

        for (int index = 1; index < tokens.Count; index++)
        {
            string[] pair = tokens[index].Split('=', StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
                continue;

            switch (pair[0])
            {
                case "id":
                    glyph.Id = int.Parse(pair[1]);
                    break;
                case "x":
                    glyph.X = int.Parse(pair[1]);
                    break;
                case "y":
                    glyph.Y = int.Parse(pair[1]);
                    break;
                case "width":
                    glyph.Width = int.Parse(pair[1]);
                    break;
                case "height":
                    glyph.Height = int.Parse(pair[1]);
                    break;

                case "xoffset":
                    glyph.XOffset = int.Parse(pair[1]);
                    break;
                case "yoffset":
                    glyph.YOffset = int.Parse(pair[1]);
                    break;
                case "xadvance":
                    glyph.XAdvance = int.Parse(pair[1]);
                    break;

                // case "page":
                //     glyph.PageIndex = byte.Parse(pair[1]);
                //     break;
                // case "chnl":
                //     glyph.ChannelMask = byte.Parse(pair[1]);
                //     break;
            }
        }

        font.Glyphs!.Add(glyph);
    }

    private static void ParseKerning(FontSource font, IReadOnlyList<string> tokens)
    {
        int first = 0;
        int second = 0;
        int amount = 0;

        for (int index = 1; index < tokens.Count; index++)
        {
            string[] pair = tokens[index].Split('=', StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
                continue;

            switch (pair[0])
            {
                case "first":
                    first = int.Parse(pair[1]);
                    break;
                case "second":
                    second = int.Parse(pair[1]);
                    break;
                case "amount":
                    amount = int.Parse(pair[1]);
                    break;
            }
        }

        if (first > 0 && second > 0 && amount != 0)
        {
            font.KerningTable!.Add(new FontKerningSource { Id = first, IdNext = second, XAdjust = amount });
        }
    }
}
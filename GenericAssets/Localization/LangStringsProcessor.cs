using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ContentBuildSystem.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;

namespace GenericAssets.Localization;

public class LangStringsProcessor : IItemProcessor
{
    private readonly IProcessorContext _context;

    public LangStringsProcessor(IProcessorContext context)
    {
        _context = context;
    }

    public bool Process(IReport? report)
    {
        // TODO: add error checks
        // - empty input
        // - exception (failed to read)
        // - failed write
        IParserConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "|", Encoding = Encoding.UTF8 };

        using FileStream file = new(_context.ItemPath, FileMode.Open);
        using TextReader reader = new StreamReader(file);
        using CsvParser parser = new(reader, config);

        parser.Read();
        int langCount = parser.Record!.Length - 1;
        List<KeyValuePair<string, Dictionary<string, string>>> languages = new(langCount);

        for (int i = 0; i < langCount; i++)
        {
            languages.Add(new KeyValuePair<string, Dictionary<string, string>>(parser.Record![i + 1], new Dictionary<string, string>()));
        }

        while (parser.Read())
        {
            string key = parser.Record![0];

            for (int i = 0; i < langCount; i++)
            {
                string value = parser.Record![i + 1];
                if (string.IsNullOrEmpty(value))
                    continue;

                languages[i].Value.Add(key, value);
            }
        }

        JsonSerializerOptions options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        Directory.CreateDirectory(Path.Combine(_context.OutputPath, _context.ItemRelativePath));

        for (int i = 0; i < langCount; i++)
        {
            report?.Info($"Building {languages[i].Key}...");
            string json = JsonSerializer.Serialize(languages[i].Value, options);

            string outputPath = Path.Combine(_context.OutputPath, _context.ItemRelativePath, $"{languages[i].Key}.json");
            File.WriteAllText(outputPath, json, Encoding.UTF8);

            _context.RegisterOutputArtifact(outputPath);
        }

        return true;
    }
}

public class LangStringsProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => false;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        throw new NotSupportedException("Lang string can have multiple output artifacts!");
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new LangStringsProcessor(context);
    }
}

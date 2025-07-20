using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ContentBuildSystem.Interfaces;
using Glslang.NET;
using SimpleTextPreprocessor;
using SimpleTextPreprocessor.ExpressionSolver;
using IReport = ContentBuildSystem.Interfaces.IReport;

namespace GenericGameAssetsProcessors.Shader;

public class ShaderGroupProcessor : IItemProcessor
{
    private static readonly JsonSerializerOptions _options = new()
    {
        AllowTrailingCommas = true,
        IncludeFields = true,
        WriteIndented = true,
    };

    private readonly IProcessorContext _context;
    private readonly ShaderGroupProcessorSettings _settings;
    private readonly Regex _extractError;

    public ShaderGroupProcessor(IProcessorContext context, ShaderGroupProcessorSettings settings)
    {
        _context = context;
        _settings = settings;

        _extractError = new Regex(@"(ERROR|WARNING): (\d+):(\d+): (.+)", RegexOptions.Singleline | RegexOptions.NonBacktracking | RegexOptions.Compiled);

        // TODO: keep dll alive
    }

    public bool Process(IReport? report)
    {
        // TODO: try/catch errors
        using FileStream file = new(_context.ItemPath, FileMode.Open);

        ShaderGroupDescription? groupDescription = JsonSerializer.Deserialize<ShaderGroupDescription>(file, _options);
        if (groupDescription?.Programs == null)
            return false;

        ShaderGroupResource shaderGroupResource = new();

        IReport? subReport = report?.CreateGroup(_context.ItemName, groupDescription.Programs.Count);

        bool result = true;
        foreach (KeyValuePair<string, ProgramDesc?> kv in groupDescription.Programs)
        {
            subReport?.Advance();

            if (kv.Value == null)
                continue;

            result = BuildProgram(kv.Value, report, out ProgramItem? item) && result;

            if (item != null)
            {
                shaderGroupResource.Add(kv.Key, item);
            }
        }

        subReport?.Finish();

        string outputDir = Path.Combine(_context.OutputPath, _context.ItemRelativePath);
        string outputPath = Path.Combine(outputDir, $"{_context.ItemName}.shaderlist");

        using FileStream output = new(outputPath, FileMode.Create);
        JsonSerializer.Serialize(output, shaderGroupResource, _options);
        _context.RegisterOutputArtifact(outputPath);

        return result;
    }

    private bool BuildProgram(ProgramDesc programDesc, IReport? report, out ProgramItem? programItem)
    {
        programItem = null;
        if (programDesc.FragmentShader == null || programDesc.VertexShader == null)
            return false;

        string itemDir = Path.GetDirectoryName(_context.ItemPath)!;

        string outDir = Path.Combine(_context.OutputPath, _context.ItemRelativePath);
        Directory.CreateDirectory(outDir);

        int crc = 0;

        string vsSrc = Path.GetFullPath(Path.Combine(itemDir, programDesc.VertexShader!.Source!));
        string vsSrcName = Path.GetFileNameWithoutExtension(vsSrc);
        string vsOut = Path.GetFullPath(Path.Combine(_context.OutputPath, _context.ItemRelativePath, $"{vsSrcName}.{crc:x8}.vert"));

        if (ProcessShader(vsSrc, vsOut, ShaderStage.Vertex, out Glslang.NET.Shader? vertexShader, report))
        {
            _context.RegisterOutputArtifact(vsSrc);
        }

        string fsSrc = Path.GetFullPath(Path.Combine(itemDir, programDesc.FragmentShader!.Source!));
        string fsSrcName = Path.GetFileNameWithoutExtension(fsSrc);
        string fsOut = Path.GetFullPath(Path.Combine(_context.OutputPath, _context.ItemRelativePath, $"{fsSrcName}.{crc:x8}.frag"));

        if (ProcessShader(fsSrc, fsOut, ShaderStage.Fragment, out Glslang.NET.Shader? fragmentShader, report))
        {
            _context.RegisterOutputArtifact(fsSrc);
        }

        bool valid = false;
        if (vertexShader != null && fragmentShader != null)
        {
            using Program program = new();
            valid = true;

            program.AddShader(vertexShader);
            program.AddShader(fragmentShader);

            if (!program.Link(MessageType.Enhanced))
            {
                valid = false;
                Console.WriteLine("GLSL linking failed");
                Console.WriteLine("-----");
                Console.WriteLine(program.GetInfoLog());
                Console.WriteLine("-----");
                Console.WriteLine(program.GetDebugLog());
                Console.WriteLine("-----");
            }
        }

        vertexShader?.Dispose();
        fragmentShader?.Dispose();

        string vsOutRel = Path.GetRelativePath(outDir, vsOut).Replace('\\', '/');
        string fsOutRel = Path.GetRelativePath(outDir, fsOut).Replace('\\', '/');
        programItem = new ProgramItem { FragmentShader = vsOutRel, VertexShader = fsOutRel, Tags = programDesc.Tags };

        return valid;
    }

    private bool ProcessShader(string srcPath, string destPath, ShaderStage stage, out Glslang.NET.Shader? shader, IReport? report)
    {
        IncludeResolver includeResolver = new(source => _context.RegisterSourceDependency(source));

        using FileStream file = File.OpenRead(srcPath);
        TextReader reader = new StreamReader(file);

        Preprocessor preprocessor = new(includeResolver, new DefaultExpressionSolver(), PreprocessorOptions.Default);
        preprocessor.AddToIgnored("version");
        preprocessor.AddToIgnored("extension");

        StringBuilder buffer = new();
        TextWriter writer = new StringWriter(buffer);

        LineNumberMapper lineNumberMapper = new();
        ReportWrapper? reportWrapper = report != null ? new ReportWrapper(report) : null;

        preprocessor.Process(includeResolver.GetFileId(srcPath), reader, writer, reportWrapper, lineNumberMapper);

        shader = MakeShader(buffer.ToString(), lineNumberMapper, stage, report);
        if (shader == null)
            return false;

        File.WriteAllText(destPath, buffer.ToString(), Encoding.UTF8);
        return true;
    }

    private Glslang.NET.Shader? MakeShader(string code, LineNumberMapper lineNumberMapper, ShaderStage stage, IReport? report)
    {
        CompilationInput input = new()
        {
            language = SourceType.GLSL,
            stage = stage,
            client = ClientType.OpenGL,
            clientVersion = TargetClientVersion.OpenGL_450,
            targetLanguage = TargetLanguage.SPV,
            targetLanguageVersion = TargetLanguageVersion.SPV_1_5,
            code = code,
            sourceEntrypoint = "main",
            defaultVersion = 330,
            defaultProfile = ShaderProfile.CoreProfile,
            forceDefaultVersionAndProfile = false,
            forwardCompatible = false,
            messages = MessageType.Enhanced,
        };

        Glslang.NET.Shader shader = new(input);

        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        shader.SetOptions(ShaderOptions.AutoMapBindings | ShaderOptions.AutoMapLocations);

        bool valid = shader.Preprocess();
        ParseMessages(shader.GetDebugLog(), lineNumberMapper, report);

        if (!valid)
        {
            shader.Dispose();
            return null;
        }

        valid = shader.Parse();
        ParseMessages(shader.GetDebugLog(), lineNumberMapper, report);

        if (!valid)
        {
            shader.Dispose();
            return null;
        }

        return shader;
    }

    private void ParseMessages(string blob, LineNumberMapper lineNumberMapper, IReport? report)
    {
        if (report == null || string.IsNullOrWhiteSpace(blob))
            return;

        foreach (string msg in blob.Split('\n'))
        {
            Match match = _extractError.Match(msg);
            if (!match.Success)
                continue;

            bool isError = match.Groups[1].Value == "ERROR";
            string msgText = match.Groups[4].Value;

            if (!int.TryParse(match.Groups[2].Value, out int column))
                column = 0;

            if (!int.TryParse(match.Groups[3].Value, out int row))
                row = 0;

            // convert to 0 based
            if (row > 0)
                row--;

            if (!lineNumberMapper.TryGetSource(row, out string filename, out int mappedRow))
            {
                filename = "source";
                mappedRow = row;
            }
            else
            {
                // strip full path for brevity
                filename = Path.GetFileName(filename);
            }

            if (isError)
                report.Error($"{filename}({mappedRow + 1},{column}) ERROR: {msgText}");
            else
                report.Warning($"{filename}({mappedRow + 1},{column}) WARN: {msgText}");
        }
    }
}

public class ShaderGroupProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => false;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        throw new NotSupportedException("ShaderGroupProcessorFactory can have multiple output artifacts!");
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new ShaderGroupProcessor(context, settings as ShaderGroupProcessorSettings ?? ShaderGroupProcessorSettings.Default);
    }
}

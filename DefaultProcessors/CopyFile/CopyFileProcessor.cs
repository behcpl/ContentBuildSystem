using System;
using System.IO;
using ContentBuildSystem.Interfaces;

namespace DefaultProcessors.CopyFile;

// force extension to lowercase?
// rename options, regex capture (name and parent folder) and use groups for name building? 
public class CopyFileProcessor : IItemProcessor
{
    private readonly IProcessorContext _context;
    private readonly string _outputPath;
    private readonly string _outputDir;

    public CopyFileProcessor(IProcessorContext context)
    {
        _context = context;
        _outputDir = Path.Combine(context.OutputPath, context.ItemRelativePath);
        _outputPath = Path.Combine(_outputDir, $"{context.ItemName}.{context.ItemExtension.ToLowerInvariant()}");
    }

    public bool Process(IReport? report)
    {
        try
        {
            Directory.CreateDirectory(_outputDir);
            
            File.Copy(_context.ItemPath, _outputPath, true);
            File.SetLastWriteTime(_outputPath, DateTime.Now);
            return true;
        }
        catch (Exception e)
        {
            // TODO: some exceptions are more like common user errors, report them without any callstack etc.
            report?.Exception(e);
            return false;
        }
    }

    public string[] GetOutputPaths()
    {
        return [_outputPath];
    }

    public string[] GetDependencies()
    {
        return [];
    }
}

public class CopyFileProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => true;

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new CopyFileProcessor(context);
    }
}
using System;
using System.IO;
using SimpleTextPreprocessor;

namespace GenericAssets.Shader;

public class IncludeResolver : IIncludeResolver
{
    private readonly Action<string> _fileRequest;

    public string GetFileId(string path) => Path.GetFullPath(path);

    public IncludeResolver(Action<string> fileRequest)
    {
        _fileRequest = fileRequest;
    }

    public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
    {
        string path2 = includeParameter.TrimStart('"').TrimEnd('"');
        string fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFileId)!, path2));

        if (!File.Exists(fullPath))
        {
            report?.Error(sourceFileId, report.CurrentLine, report.CurrentColumn, "File '" + fullPath + "' does not exist!");
            newFileId = null;
            reader = null;
            return false;
        }

        try
        {
            newFileId = fullPath;
            reader = new StreamReader(fullPath);
        }
        catch (Exception ex)
        {
            report?.Exception(sourceFileId, report.CurrentLine, report.CurrentColumn, ex);
            newFileId = null;
            reader = null;
            return false;
        }

        _fileRequest(fullPath);
        return true;
    }
}

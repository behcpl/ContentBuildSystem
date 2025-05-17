using System;
using SimpleTextPreprocessor;

namespace GenericAssets.Shader;

public class ReportWrapper : IReport
{
    private readonly ContentBuildSystem.Interfaces.IReport _report;

    public string CurrentFileId { get; set; }
    public int CurrentLine { get; set; }
    public int CurrentColumn { get; set; }

    public ReportWrapper(ContentBuildSystem.Interfaces.IReport report)
    {
        CurrentFileId = string.Empty;

        _report = report;
    }

    public void Error(string message)
    {
        _report.Error(message);
    }

    public void Exception(Exception e)
    {
        _report.Exception(e);
    }
}

using System.Collections.Generic;

namespace ContentBuildSystem.Interfaces;

public interface IProcessorContext
{
    string ItemPath { get; }
    string ItemName { get; }
    string ItemExtension { get; }
    string ItemRelativePath { get; }
    string GroupPath { get; }
    string ProjectPath { get; }
    string OutputPath { get; }
    string TempPath { get; }

    // register file that contributes to output artifacts
    void RegisterSourceDependency(string path);

    // registered folder
    void RegisterSourceFolderDependency(string path, bool recursive, IReadOnlyList<string>? extensions);

    void RegisterOutputArtifact(string path);
    void RegisterOutputDependency(string path);
}

public interface IItemProcessor
{
    bool Process(IReport? report);
}

public interface IItemProcessorFactory
{
    // only single input & output, no need to create temporary files
    bool SimpleProcessor { get; }

    // valid if SimpleProcessor, allow to skip processing
    string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings);

    IItemProcessor Create(IProcessorContext context, object? settings);
}
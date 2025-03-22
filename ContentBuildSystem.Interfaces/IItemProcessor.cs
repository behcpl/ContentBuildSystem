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
}

public interface IItemProcessor
{
    bool Process(IReport? report);

    string[] GetOutputPaths();
    string[] GetDependencies();
}

public interface IItemProcessorFactory
{
    // only single input & output, no need to create temporary files
    bool SimpleProcessor { get; }
    
    IItemProcessor Create(IProcessorContext context, object? settings);
}
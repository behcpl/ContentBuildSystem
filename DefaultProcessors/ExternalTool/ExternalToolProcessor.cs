using System.IO;
using ContentBuildSystem.Interfaces;

namespace DefaultProcessors.ExternalTool;

public class ExternalToolProcessor : IItemProcessor
{
    public ExternalToolProcessor(IProcessorContext context) { }

    public bool Process(IReport? report)
    {
        // TODO: call some external executable using correct paths
        // TODO: option to move result from some path into configured one
        return true;
    }
}

public class ExternalToolProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => true;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        // TODO: get this somehow from settings?
        return Path.Combine(context.OutputPath, context.ItemRelativePath, $"{context.ItemName}.{context.ItemExtension}");
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new ExternalToolProcessor(context);
    }
}
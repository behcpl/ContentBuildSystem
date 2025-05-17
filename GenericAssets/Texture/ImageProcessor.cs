using System;
using ContentBuildSystem.Interfaces;

namespace GenericAssets.Texture;

public class ImageProcessor : IItemProcessor
{
    public bool Process(IReport? report)
    {
        throw new NotImplementedException();
    }

    public string[] GetOutputPaths()
    {
        throw new NotImplementedException();
    }

    public string[] GetDependencies()
    {
        throw new NotImplementedException();
    }
}

public class ImageProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => false;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        throw new NotImplementedException();
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new ImageProcessor();
    }
}

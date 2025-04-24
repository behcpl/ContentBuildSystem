using ContentBuildSystem.Interfaces;

namespace GenericAssets.Texture;

public class ImageProcessor : IItemProcessor
{
    public bool Process(IReport? report)
    {
        throw new System.NotImplementedException();
    }

    public string[] GetOutputPaths()
    {
        throw new System.NotImplementedException();
    }

    public string[] GetDependencies()
    {
        throw new System.NotImplementedException();
    }
}

public class ImageProcessorFactory : IItemProcessorFactory
{
    public bool SimpleProcessor => false;

    public string GetDefaultOutputArtifactPath(IProcessorContext context, object? settings)
    {
        throw new System.NotImplementedException();
    }

    public IItemProcessor Create(IProcessorContext context, object? settings)
    {
        return new ImageProcessor();
    }
}
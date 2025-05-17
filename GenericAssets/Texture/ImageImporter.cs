using ContentBuildSystem.Interfaces;

namespace GenericAssets.Texture;

public class ImageImporter : IImporter<ImageData>
{
    public bool TryImport(string path, out ImageData? result, IReport? report)
    {
        throw new System.NotImplementedException();
    }
}
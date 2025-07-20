using ContentBuildSystem.Interfaces;
using GenericGameAssetsProcessors.ImageImporter;

namespace GenericGameAssetsProcessors.Services;

public interface IDebugImage
{
    void SaveAsPng(string filepath, ImageData image, IReport? report);
}

using GenericAssets.Texture;

namespace GenericAssets.Services;

public interface IDebugImage
{
    // TODO: create editable image class, and use it here
    void SaveAsPng(ImageData image);
}

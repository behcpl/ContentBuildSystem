using System;
using GenericGameAssets;
using GenericGameAssetsProcessors.ImageImporter;

namespace GenericGameAssetsProcessors.Texture;

internal class TextureAssetConverter
{
    // USE CASES:
    // #1 dump single image as texture
    // #2 single texture with mip maps <- more than one ImageData
    // #3 cubemap/volume assembled from multiple ImageData (can have mipmaps) <- no longer trivial case
    // NOTE: some processors make little sense for cube maps or volume maps
    
    public void Convert(out TextureAsset textureAsset, ImageData imageData)
    {
        throw new NotImplementedException();
    }
}

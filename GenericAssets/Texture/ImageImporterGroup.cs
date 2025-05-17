using System;
using System.Collections.Generic;
using System.IO;
using ContentBuildSystem.Interfaces;

namespace GenericAssets.Texture;

public class ImageImporterGroup : IImporterGroup<ImageData>
{
    private readonly Dictionary<string, IImporter<ImageData>> _mapExtensions;

    private readonly List<string> _extensions;

    public ImageImporterGroup()
    {
        _mapExtensions = new Dictionary<string, IImporter<ImageData>>();
        _extensions = [ ];
    }

    public bool TryImport(string path, out ImageData? result, IReport? report)
    {
        string ext = Path.GetExtension(path.AsSpan()).TrimStart('.').ToString().ToLowerInvariant();
        if (!_mapExtensions.TryGetValue(ext, out IImporter<ImageData>? importer))
        {
            report?.Error($"Extension '{ext}' not supported!");
            result = null;
            return false;
        }

        return importer.TryImport(path, out result, report);
    }

    public void AddImporter(IImporter<ImageData> importer, IEnumerable<string> fileExtensions)
    {
        HashSet<string> set = [ ];
        foreach (string oldExt in _extensions)
        {
            set.Add(oldExt);
        }

        foreach (string newExt in fileExtensions)
        {
            _mapExtensions[newExt] = importer;
            set.Add(newExt);
        }

        _extensions.Clear();
        _extensions.AddRange(set);
    }

    public IEnumerable<string> GetSupportedExtensions()
    {
        return _extensions;
    }
}

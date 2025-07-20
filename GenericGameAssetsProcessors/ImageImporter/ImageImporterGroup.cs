using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ContentBuildSystem.Interfaces;

namespace GenericGameAssetsProcessors.ImageImporter;

public class ImageImporterGroup : IImporterGroup<ImageData>, IParameterResolver<string[]>
{
    private const string _TAG = "image-extensions";
    
    private readonly Dictionary<string, IImporter<ImageData>> _mapExtensions;

    private readonly HashSet<string> _extensions;

    public ImageImporterGroup()
    {
        _mapExtensions = new Dictionary<string, IImporter<ImageData>>();
        _extensions = [ ];
    }

    public bool TryImport(string path, [MaybeNullWhen(false)] out ImageData result, IReport? report)
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
        foreach (string newExt in fileExtensions)
        {
            _mapExtensions[newExt] = importer;
            _extensions.Add(newExt);
        }
    }

    public bool TryGetParameter(string tag, [MaybeNullWhen(false)] out string[] value)
    {
        if (tag == _TAG)
        {
            value = _extensions.ToArray();
            return true;
        }
        
        value = null;
        return false;
    }
}

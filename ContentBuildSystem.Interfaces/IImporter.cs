using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ContentBuildSystem.Interfaces;

public interface IImporter<T>
{
    bool TryImport(string path, [MaybeNullWhen(false)] out T result, IReport? report);
}

public interface IImporterGroup<T> : IImporter<T>
{
    void AddImporter(IImporter<T> importer, IEnumerable<string> fileExtensions);
}

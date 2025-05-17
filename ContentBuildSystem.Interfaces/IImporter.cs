using System.Collections.Generic;

namespace ContentBuildSystem.Interfaces;

public interface IImporter<T>
{
    bool TryImport(string path, out T? result, IReport? report);
}

public interface IImporterGroup<T> : IImporter<T>
{
    void AddImporter(IImporter<T> importer, IEnumerable<string> fileExtensions);

    IEnumerable<string> GetSupportedExtensions();
}

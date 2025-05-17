using System.Diagnostics.CodeAnalysis;

namespace ContentBuildSystem.Interfaces;

public interface IServiceRepository
{
    public void Add<T>(T service);
    public bool TryGet<T>([MaybeNullWhen(false)] out T service);
}

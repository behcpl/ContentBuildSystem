using System.Diagnostics.CodeAnalysis;

namespace ContentBuildSystem.Interfaces;

public interface IParameterResolver<T>
{
    bool TryGetParameter(string tag, [MaybeNullWhen(false)] out T value);
}

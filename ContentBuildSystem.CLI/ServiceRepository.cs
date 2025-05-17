using System;
using System.Collections.Generic;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem.CLI;

public sealed class ServiceRepository : IServiceRepository
{
    private readonly Dictionary<Type, object> _services;

    public ServiceRepository()
    {
        _services = new Dictionary<Type, object>();
    }

    public void Add<T>(T service)
    {
        _services[typeof(T)] = service!;
    }

    public bool TryGet<T>(out T service)
    {
        if (_services.TryGetValue(typeof(T), out object? serviceObj))
        {
            service = (T)serviceObj;
            return true;
        }

        service = default!;
        return false;
    }
}
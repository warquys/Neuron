﻿using System;
using System.Collections.Generic;
using Neuron.Core.Meta;
using Ninject;

namespace Neuron.Core.Dependencies;

/// <summary>
/// Utility class for analyzing Ninject-Bindings.
/// </summary>
public static class KernelDependencyResolver
{
    private static readonly Dictionary<Type, List<Type>> DependencyCache = new();

    /// <summary>
    /// Retrieves all Ninject-Binding types which are defined via [Inject] Attributes
    /// for the given type.
    /// </summary>
    public static IEnumerable<Type> GetPropertyDependencies(Type type)
    {
        if (DependencyCache.TryGetValue(type, out var cached)) return cached;
        
        var meta = MetaType.Analyze(type, ignoreMeta: true);
        var list = new List<Type>();
        foreach (var property in meta.Properties)
        {
            if (property.TryGetAttribute<InjectAttribute>(out var x))
            {
                list.Add(property.Property.PropertyType);
            }
        }
        DependencyCache[type] = list;
        return list;
    }
}
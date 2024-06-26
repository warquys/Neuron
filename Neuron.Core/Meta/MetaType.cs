﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace Neuron.Core.Meta;

public class MetaType
{
    public Type Type { get; set; }
    public object[] Attributes { get; set; }
    public MetaMethod[] Methods { get; set; }
    public MetaProperty[] Properties { get; set; }

    public bool TryGetAttribute<T>(out T output)
    {
        output = default;
        if (!TryGetAttributes<T>(out var attributes))
            return false;
        output = attributes[0];
        return true;
    }

    public bool TryGetAttributes<T>(out T[] output)
    {
        output = default;
        var matching = Attributes.OfType<T>().ToArray();
        if (matching.Length == 0) return false;
        output = matching;
        return true;
    }

    public bool Is<T>() => typeof(T).IsAssignableFrom(Type);

    public object New() => Activator.CreateInstance(Type);

    protected bool Equals(MetaType other)
    {
        return Equals(Type, other.Type);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MetaType) obj);
    }

    public override int GetHashCode()
    {
        return (Type != null ? Type.GetHashCode() : 0);
    }

    /// <summary>
    /// Checks if the specified type qualifies as being meta, and then parses it as <see cref="MetaType"/> or return <see langword="null"/>.
    /// Having at least one of following properties makes an object meta:
    /// <list type="number">
    /// <item/> Have a <see cref="MetaAttribute"/> or another Attribute extending <see cref="MetaAttributeBase"/>
    /// <item/> Extend a class which is has an <see cref="MetaAttribute"/> or another attribute extending <see cref="MetaAttributeBase"/>
    /// <item/> Implement the <see cref="IMetaObject"/> or derived interfaces.
    /// <item/> Have properties which have a <see cref="MetaAttribute"/> or another attribute extending <see cref="MetaAttributeBase"/>
    /// <item/> Have methods which have a <see cref="MetaAttribute"/> or another attribute extending <see cref="MetaAttributeBase"/>
    /// </list>
    /// <br/>
    /// </summary>
    /// <param name="type">the type to analyze</param>
    /// <returns>the analyzed <see cref="MetaType"/> or <see langword="null"/></returns>
    public static MetaType TryGetMetaType(Type type)
    {
        var keepType = false;
        var metaAttributesList = type.GetCustomAttributes(true)
            .OfType<MetaAttributeBase>().ToList();
        var interfaceAttributes = ReflectionUtils
            .ResolveInterfaceAttributes(type)
            .OfType<MetaAttributeBase>();
        metaAttributesList.AddRange(interfaceAttributes);
        var metaAttributes = metaAttributesList.ToArray();
        if (metaAttributes.Length != 0) keepType = true;
        
        var allAttributesList = type.GetCustomAttributes(true).ToList();
        var allInterfaceAttributes = ReflectionUtils
            .ResolveInterfaceAttributes(type);
        allAttributesList.AddRange(allInterfaceAttributes);
            
        var metaMethods = new List<MetaMethod>();
        var metaProperties = new List<MetaProperty>();
                        
        foreach (var x in type.GetRuntimeMethods())
        {
            var methodMetaAttributes = x
                .GetCustomAttributes(true).OfType<MetaAttributeBase>().ToArray();
            
            if (methodMetaAttributes.Length != 0) keepType = true;
                            
            metaMethods.Add(new MetaMethod
            {
                Method = x,
                Attributes = x.GetCustomAttributes(true)
            });
        }
                        
        foreach (var x in type.GetRuntimeProperties())
        {
            var propertyMetaAttributes = x
                .GetCustomAttributes(true).OfType<MetaAttributeBase>().ToArray();
                            
            if (propertyMetaAttributes.Length != 0) keepType = true;
                            
            metaProperties.Add(new MetaProperty
            {
                Property = x,
                Attributes = x.GetCustomAttributes(true)
            });
        }
            
        if (!keepType) return null;
        return new MetaType
        {
            Type = type,
            Attributes = allAttributesList.ToArray(),
            Methods = metaMethods.ToArray(),
            Properties = metaProperties.ToArray()
        };
    }

    public static MemoryCache _typeCache = new(new MemoryCacheOptions());
    
    public static MetaType WrapMetaType(Type type)
    {
        if (_typeCache.TryGetValue(type, out var cached)) 
            return (MetaType) cached;
        
        var allAttributesList = type.GetCustomAttributes(true).ToList();
        var allInterfaceAttributes = ReflectionUtils
            .ResolveInterfaceAttributes(type);
        allAttributesList.AddRange(allInterfaceAttributes);
            
        var metaMethods = new List<MetaMethod>();
        var metaProperties = new List<MetaProperty>();
                        
        foreach (var x in type.GetRuntimeMethods())
        {
            metaMethods.Add(new MetaMethod
            {
                Method = x,
                Attributes = x.GetCustomAttributes(true)
            });
        }
                        
        foreach (var x in type.GetRuntimeProperties())
        {
            metaProperties.Add(new MetaProperty
            {
                Property = x,
                Attributes = x.GetCustomAttributes(true)
            });
        }
            
        var meta = new MetaType
        {
            Type = type,
            Attributes = allAttributesList.ToArray(),
            Methods = metaMethods.ToArray(),
            Properties = metaProperties.ToArray()
        };
        _typeCache.Set(type, meta, TimeSpan.FromSeconds(30));
        return meta;
    }
}
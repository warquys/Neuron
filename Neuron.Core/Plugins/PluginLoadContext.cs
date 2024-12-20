﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Neuron.Core.Meta;

namespace Neuron.Core.Plugins;

public class PluginLoadContext : ILoadingContext
{
    public Assembly Assembly { get; set; }
    public MetaBatchReference Batch { get; set; }
    public PluginAttribute Attribute { get; set; }
    public Type PluginType { get; set; }
    public Plugin Plugin { get; set; }
    public PluginLifecycle Lifecycle { get; set; }
    public List<IMetaBinding> MetaBindings { get; set; }

    public override string ToString() => PluginType.FullName;


    ILoadableAttributeContext ILoadingContext.Attribute => Attribute;
    Type ILoadingContext.Type => PluginType;
    ILifeCycle ILoadingContext.Lifecycle => Lifecycle;
}
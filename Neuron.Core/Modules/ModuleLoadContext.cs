using System;
using System.Collections.Generic;
using System.Reflection;
using Neuron.Core.Dependencies;
using Neuron.Core.Meta;

namespace Neuron.Core.Modules;

public class ModuleLoadContext : SimpleDependencyHolderBase, ILoadingContext
{
    public Assembly Assembly { get; set; }
    public MetaBatchReference Batch { get; set; }
    public ModuleAttribute Attribute { get; set; }
    public Type[] ModuleDependencies { get; set; }
    public Type ModuleType { get; set; }
    public Module Module { get; set; }
    public ModuleLifecycle Lifecycle { get; set; }
    public List<IMetaBinding> MetaBindings { get; set; }

    public override IEnumerable<object> Dependencies => ModuleDependencies;
    public override object Dependable => ModuleType;

    public override string ToString() => ModuleType.FullName;

    ILoadableAttributeContext ILoadingContext.Attribute => Attribute;
    Type ILoadingContext.Type => ModuleType;
    ILifeCycle ILoadingContext.Lifecycle => Lifecycle;

}
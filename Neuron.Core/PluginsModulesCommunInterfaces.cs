using Neuron.Core.Meta;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Neuron.Core;

public interface ILoadingContext
{
    ILoadableAttributeContext Attribute { get;  }

    Assembly Assembly { get; }
    
    MetaBatchReference Batch { get; }
    
    ILifeCycle Lifecycle { get; }

    Type Type { get; }

    List<IMetaBinding> MetaBindings { get; }

}

public interface ILoadableAttributeContext
{
    string Name { get; set; }
    string Description { get; set; }
    string Version { get; set; }

    string Author { get; set; }
    string Website { get; set; }
    string Repository { get; set; }

}

public interface ILifeCycle
{
    void EnableSignal();
    void DisableSignal();
}
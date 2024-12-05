using System;
using Neuron.Core.Meta;
using Neuron.Core.Modules;
using Neuron.Core.Plugins;
using Ninject;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Linq;

namespace Neuron.Core;

[Serializable]
public class IndefiniteExtensionPointException: Exception
{
    public IndefiniteExtensionPointException() { }
    public IndefiniteExtensionPointException(string message) : base(message) { }
    public IndefiniteExtensionPointException(string message, Exception inner) : base(message, inner) { }
    protected IndefiniteExtensionPointException(
      SerializationInfo info,
      StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ModuleOrPluginConflictException : Exception
{
    private static readonly ModuleManager _module;
    private static readonly PluginManager _plugin;

    public ILoadingContext Conflict1 { get; }
    public ILoadingContext Conflict2 { get; }

    static ModuleOrPluginConflictException()
    {
        _module = Globals.Kernel.Get<ModuleManager>();
        _plugin = Globals.Kernel.Get<PluginManager>();
    }

    public ModuleOrPluginConflictException() 
    {
        Conflict1 = new UnknowContext(null);
        Conflict2 = new UnknowContext(null);
    }

    public ModuleOrPluginConflictException(string message, ILoadingContext conflict1, ILoadingContext conflict2) : base(message)
    {
        Conflict1 = conflict1;
        Conflict2 = conflict2;
    }

    public ModuleOrPluginConflictException(string message, ILoadingContext conflict1, ILoadingContext conflict2, Exception inner) : base(message, inner)
    {
        Conflict1 = conflict1;
        Conflict2 = conflict2;
    }

    protected ModuleOrPluginConflictException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public static ModuleOrPluginConflictException Build(object conflict1, object conflict2, string message)
        => Build(conflict1.GetType(), conflict2.GetType(), message);

    public static ModuleOrPluginConflictException Build(Type conflict1, Type conflict2, string message)
    {
        ILoadingContext context1;
        ILoadingContext context2;

        context1 = _plugin.Plugins.Find(p => p.Assembly == conflict1.Assembly);
        if (context1 == null)
            context1 = _module.GetAllModules().FirstOrDefault(p => p.Assembly == conflict1.Assembly);
        if (context1 == null)
            context1 = new UnknowContext(conflict1.Assembly);

        context2 = _plugin.Plugins.Find(p => p.Assembly == conflict2.Assembly);
        if (context2 == null)
            context2 = _module.GetAllModules().FirstOrDefault(p => p.Assembly == conflict2.Assembly);
        if (context2 == null)
            context2 = new UnknowContext(conflict2.Assembly);

        return new ModuleOrPluginConflictException(message, context1, context2);
    }


    public class UnknowContext : ILoadingContext
    {
        public UnknowContext(Assembly assembly)
        {
            Assembly = assembly;
        }

        public ILoadableAttributeContext Attribute { get; } = new UnknowAttributeContext();

        public Assembly Assembly { get; }

        public MetaBatchReference Batch => null;

        public ILifeCycle Lifecycle => null;

        public Type Type => null;

        public List<IMetaBinding> MetaBindings => null;
    }

    public class UnknowAttributeContext : ILoadableAttributeContext
    {
        public string Name { get; set; } = "Unknow";
        public string Description { get; set; } = "Unknow";
        public string Version { get; set; } = "Unknow";
        public string Author { get; set; } = "Unknow";
        public string Website { get; set; } = "Unknow";
        public string Repository { get; set; } = "Unknow";
    }
}
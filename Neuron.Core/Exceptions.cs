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
public class ModuleOrPluginConflicException : Exception
{
    private static readonly ModuleManager _module;
    private static readonly PluginManager _plugin;

    public ILoadingContext Conflic1 { get; }
    public ILoadingContext Conflic2 { get; }

    static ModuleOrPluginConflicException()
    {
        _module = Globals.Kernel.Get<ModuleManager>();
        _plugin = Globals.Kernel.Get<PluginManager>();
    }


    public ModuleOrPluginConflicException() { }

    public ModuleOrPluginConflicException(string message, ILoadingContext conflic1, ILoadingContext conflic2) : base(message)
    {
        Conflic1 = conflic1;
        Conflic2 = conflic2;
    }

    public ModuleOrPluginConflicException(string message, ILoadingContext conflic1, ILoadingContext conflic2, Exception inner) : base(message, inner)
    {
        Conflic1 = conflic1;
        Conflic2 = conflic2;
    }

    protected ModuleOrPluginConflicException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public static ModuleOrPluginConflicException Build(object conflic1, object conflic2, string message)
        => Build(conflic1.GetType(), conflic2.GetType(), message);

    public static ModuleOrPluginConflicException Build(Type conflic1, Type conflic2, string message)
    {
        ILoadingContext context1;
        ILoadingContext context2;

        context1 = _plugin.Plugins.Find(p => p.Assembly == conflic1.Assembly);
        if (context1 == null)
            context1 = _module.GetAllModules().FirstOrDefault(p => p.Assembly == conflic1.Assembly);
        if (context1 == null)
            context1 = new UnknowContext(conflic1.Assembly);

        context2 = _plugin.Plugins.Find(p => p.Assembly == conflic2.Assembly);
        if (context2 == null)
            context2 = _module.GetAllModules().FirstOrDefault(p => p.Assembly == conflic2.Assembly);
        if (context2 == null)
            context2 = new UnknowContext(conflic2.Assembly);

        return new ModuleOrPluginConflicException(message, context1, context2);
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
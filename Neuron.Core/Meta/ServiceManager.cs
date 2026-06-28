using System;
using System.Collections.Generic;
using Neuron.Core.Dependencies;
using Ninject;

namespace Neuron.Core.Meta;

/// <summary>
/// Neuron service system.
/// </summary>
public class ServiceManager
{
    private IKernel _kernel;
    private MetaManager _meta;
    public List<ServiceRegistration> Services { get; set; }

    public ServiceManager(IKernel kernel, MetaManager meta)
    {
        _kernel = kernel;
        _meta = meta;
        _meta.MetaGenerateBindings.Subscribe(MetaDelegate);
        Services = new List<ServiceRegistration>();
    }

    internal void MetaDelegate(MetaGenerateBindingsEvent args)
    {
        if (!args.MetaType.TryGetAttribute<AutomaticAttribute>(out _)) return;
        if (!args.MetaType.Is<Service>()) return;

        var serviceType = args.MetaType.Type;
        if (args.MetaType.TryGetAttribute<ServiceInterfaceAttribute>(out var serviceInterface))
        {
            serviceType = serviceInterface.ServiceType;
        }
        var obj = new ServiceRegistration()
        {
            MetaType = args.MetaType,
            ServiceType = serviceType
        };
        args.Outputs.Add(obj);
    }


    public ServiceRegistration RegisterService(Service service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        var metaType = MetaType.TryGetMetaType(service.GetType());
        var serviceType = metaType.Type;
        if (metaType.TryGetAttribute<ServiceInterfaceAttribute>(out var serviceInterface))
        {
            serviceType = serviceInterface.ServiceType;
        }
        var registration = new ServiceRegistration()
        {
            MetaType = metaType,
            ServiceType = serviceType
        };
        _kernel.Bind(registration.ServiceType).ToConstant(service).InSingletonScope();
        Services.Add(registration);
        return registration;
    }

    /// <summary>
    /// Binds the service registration to the ninject kernel and the local registry.
    /// </summary>
    public void BindService(ServiceRegistration service)
    {
        _kernel.Bind(service.ServiceType).To(service.MetaType.Type).InSingletonScope();
        _kernel.Get(service.ServiceType);
        Services.Add(service);
    }

    
    /// <summary>
    /// Unbinds the service registration from the ninject kernel and the local registry.
    /// </summary>
    public void UnbindService(ServiceRegistration service)
    {
        if (Services.Contains(service)) Services.Remove(service);
        var serviceType = service.MetaType.Type;
        if (service.MetaType.TryGetAttribute<ServiceInterfaceAttribute>(out var serviceInterface))
        {
            serviceType = serviceInterface.ServiceType;
        }
        _kernel.Unbind(serviceType);
    }
}

/// <summary>
/// Data-Holder for service registrations.
/// </summary>
public class ServiceRegistration : SimpleDependencyHolderBase, IMetaBinding
{
    public Type ServiceType { get; set; }
    public MetaType MetaType { get; set; }

    public override IEnumerable<object> Dependencies => KernelDependencyResolver.GetTypeDependencies(MetaType.Type);
    public override object Dependable => ServiceType;

    public override string ToString() => ServiceType.Name;
    public IEnumerable<Type> PromisedServices => new Type[] {};
}
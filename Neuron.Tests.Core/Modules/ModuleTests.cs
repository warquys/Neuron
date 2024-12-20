﻿using System;
using System.Linq;
using Neuron.Core;
using Neuron.Core.Dependencies;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Neuron.Core.Modules;
using Neuron.Core.Platform;
using Ninject;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Neuron.Tests.Core.Modules;

public class ModuleTests
{
    private readonly ITestOutputHelper _output;
    private readonly IPlatform _neuron;

    public ModuleTests(ITestOutputHelper output)
    {
        this._output = output;
        _neuron = NeuronMinimal.DebugHook(output.WriteLine);
    }

    [Fact]
    public void TestBasicModuleSetup()
    {
        var logger = _neuron.NeuronBase.Kernel.Get<NeuronLogger>();
        var kernel = new StandardKernel();
        kernel.BindSimple(logger);
            
        var metaManager = new MetaManager(logger);
        var serviceManager = new ServiceManager(kernel, metaManager);
        var moduleManager = new ModuleManager(_neuron.NeuronBase, metaManager, logger, kernel, serviceManager);
        //moduleManager.LoadModule(new []{typeof(ModuleC)});
        moduleManager.LoadModule(new []{typeof(ModuleB), typeof(ServiceB)});
        moduleManager.LoadModule(new []{typeof(ModuleD)});
        moduleManager.LoadModule(new []{typeof(ModuleA), typeof(ServiceA), typeof(ServiceASub)}); // Out of order for test reasons
            
        _output.WriteLine(String.Join(":", KernelDependencyResolver.GetTypeDependencies(typeof(ServiceA))));
        _output.WriteLine(String.Join(":", KernelDependencyResolver.GetTypeDependencies(typeof(ServiceB))));
        _output.WriteLine(String.Join(":", KernelDependencyResolver.GetTypeDependencies(typeof(ModuleB))));
        _output.WriteLine(kernel.GetBindings(typeof(Random)).Any().ToString());
            
        Assert.False(moduleManager.IsLocked);
        moduleManager.ActivateModules();
        Assert.True(moduleManager.IsLocked);

        _output.WriteLine(kernel.GetBindings(typeof(Random)).Any().ToString());
        moduleManager.EnableAll();
        _output.WriteLine(kernel.GetBindings(typeof(Random)).Any().ToString());
            
        var moduleB = kernel.Get<ModuleB>();
        var serviceB = kernel.Get<ServiceB>();
            
        Assert.NotNull(moduleB.A);
        Assert.NotNull(serviceB.A);
        Assert.NotNull(serviceB.ServiceA);
            
        Assert.Single(kernel.GetBindings(typeof(ServiceA)));
        Assert.Single(kernel.GetBindings(typeof(ServiceB)));
        Assert.Single(kernel.GetBindings(typeof(ModuleA)));
        Assert.Single(kernel.GetBindings(typeof(ModuleB)));
            
        moduleManager.DisableAll();
            
        Assert.Empty(kernel.GetBindings(typeof(ServiceA)));
        Assert.Empty(kernel.GetBindings(typeof(ServiceB)));
        Assert.Empty(kernel.GetBindings(typeof(ModuleA)));
        Assert.Empty(kernel.GetBindings(typeof(ModuleB)));
    }
}
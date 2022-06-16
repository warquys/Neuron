﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neuron.Core.Logging.Processing;

namespace Neuron.Core.Logging;

public class LoggerController
{
    public ILogFormatter Formatter = new DefaultLogFormatter();
    public List<ILogRender> Renderers = new ILogRender[] {}.ToList();
    public LogLevel MinimumLevel = LogLevel.Verbose;

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < MinimumLevel) return;
        if (logEvent.Args == null) logEvent.Args = new List<object>();
        var output = Formatter.Resolve(logEvent);
        foreach (var logRender in Renderers)
        {
            logRender.Render(output);
        }
    }

    public ILogger GetLogger() => new DelegateLogger(this, Assembly.GetCallingAssembly().GetName().Name);
    public ILogger GetLogger<T>() => new DelegateLogger(this, typeof(T).Name);
    public ILogger GetLogger(Type type) => new DelegateLogger(this, type.Name);
}

public class DelegateLogger : LoggerBase
{
    private LoggerController _controller;
    private string _caller;

    public DelegateLogger(LoggerController controller, string caller)
    {
        _controller = controller;
        _caller = caller;
    }

    public override void Log(LogLevel level, string template, object[] args, bool isPure = false)
    {
        _controller.Emit(new LogEvent(level, template, args?.ToList(), DateTime.Now, _caller, isPure));
    }
}
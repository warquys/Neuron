using System;
using Neuron.Core.Logging.Processing;
using Neuron.Core.Logging.Utils;

namespace Neuron.Core.Logging;

/// <summary>
/// Neurons logging service.
/// </summary>
public class NeuronLogger
{
    public readonly LoggerController logger;
    public readonly NeuronBase neuronBase;

    public NeuronLogger(NeuronBase neuronBase)
    {
        this.neuronBase = neuronBase;
        logger = new LoggerController();
        logger.MinimumLevel = this.neuronBase.Configuration.Logging.LogLevel;
        if (this.neuronBase.Platform.Configuration.EnableConsoleLogging)
            logger.Renderers.Add(new ConsoleRender());
        if (this.neuronBase.Platform.Configuration.ConsoleWidth != -1)
            ConsoleWrapper.WidthOverride = this.neuronBase.Platform.Configuration.ConsoleWidth;
        if (this.neuronBase.Platform.Configuration.LogEventSink != null)
            logger.Renderers.Add(this.neuronBase.Platform.Configuration.LogEventSink);
    }

    /// <summary>
    /// Retrieves a logger which has the specified type set as its caller.
    /// </summary>
    public ILogger GetLogger<T>() => logger.GetLogger<T>();

    /// <summary>
    /// Retrieves a logger which has the specified type set as its caller.
    /// </summary>
    public ILogger GetLogger(Type callingType) => logger.GetLogger(callingType);

    /// <summary>
    /// Retrieves a logger which has the specified object set as its caller.
    /// </summary>
    public ILogger GetLogger(object owner) => logger.GetLogger(owner.GetType());

    /// <summary>
    /// Retrieves a logger which has the specified its caller.
    /// Use it in the case of static methode or dynamic.
    /// </summary>
    public ILogger GetLogger(string caller) => logger.GetLogger(caller);

    /// <summary>
    /// Retrieves a logger using <see cref="Globals"/> which has the specified type set as its caller.
    /// </summary>
    public static ILogger For<T>() => Globals.Get<NeuronLogger>().GetLogger<T>();

    /// <summary>
    /// Retrieves a logger using <see cref="Globals"/> which has the specified type set as its caller.
    /// </summary>
    public static ILogger For(Type type) => Globals.Get<NeuronLogger>().GetLogger(type);
}

public static class NeuronLogger<TName>
{
    private static ILogger _logger;
    public static ILogger Logger => _logger ??= NeuronLogger.For<TName>();

    /// <summary>
    /// Logs a plain string
    /// </summary>
    public static void Verbose(string message) => Logger.Verbose(message);

    /// <summary>
    /// Logs a message template with a multiple parameters.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Verbose(string template, params object[] args) => Logger.Verbose(template, args);

    /// <summary>
    /// Logs a plain string
    /// </summary>
    public static void Debug(string message) => Logger.Debug(message);

    /// <summary>
    /// Logs a message template with a multiple parameters.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Debug(string template, params object[] args) => Logger.Debug(template, args);

    /// <summary>
    /// Logs a plain string
    /// </summary>
    public static void Info(string message) => Logger.Debug(message);

    /// <summary>
    /// Logs a message template with a multiple parameters.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Info(string template, params object[] args) => Logger.Info(template, args);

    /// <summary>
    /// Logs a plain string
    /// </summary>
    public static void Warn(string message) => Logger.Warn(message);

    /// <summary>
    /// Logs a message template with a multiple parameters.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Warn(string template, params object[] args) => Logger.Warn(template, args);

    /// <summary>
    /// Logs a plain string
    /// </summary>
    public static void Error(string message) => Logger.Error(message);

    /// <summary>
    /// Logs a message template with a multiple parameters.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Error(string template, params object[] args) => Logger.Error(template, args);

    /// <summary>
    /// Logs a plain string
    /// </summary>
    public static void Fatal(string message) => Logger.Fatal(message);

    /// <summary>
    /// Logs a message template with a multiple parameters.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Fatal(string template, params object[] args) => Logger.Fatal(template, args);

    /// <summary>
    /// Logs a message template with a single parameter.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Verbose(string template, object arg0) => Logger.Verbose(template, arg0);

    /// <summary>
    /// Logs a message template with a single parameter.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Debug(string template, object arg0) => Logger.Debug(template, arg0);

    /// <summary>
    /// Logs a message template with a single parameter.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Info(string template, object arg0) => Logger.Info(template, arg0);

    /// <summary>
    /// Logs a message template with a single parameter.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Warn(string template, object arg0) => Logger.Warn(template, arg0);

    /// <summary>
    /// Logs a message template with a single parameter.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Error(string template, object arg0) => Logger.Error(template, arg0);

    /// <summary>
    /// Logs a message template with a single parameter.
    /// For details on the templating format see <see cref="Log"/>
    /// </summary>
    public static void Fatal(string template, object arg0) => Logger.Fatal(template, arg0);

    /// <summary>
    /// Logs an object using the default object tokenizer
    /// </summary>
    public static void Verbose(object obj) => Logger.Verbose(obj);

    /// <summary>
    /// Logs an object using the default object tokenizer
    /// </summary>
    public static void Debug(object obj) => Logger.Debug(obj);

    /// <summary>
    /// Logs an object using the default object tokenizer
    /// </summary>
    public static void Info(object obj) => Logger.Info(obj);

    /// <summary>
    /// Logs an object using the default object tokenizer
    /// </summary>
    public static void Warn(object obj) => Logger.Warn(obj);

    /// <summary>
    /// Logs an object using the default object tokenizer
    /// </summary>
    public static void Error(object obj) => Logger.Error(obj);

    /// <summary>
    /// Logs an object using the default object tokenizer
    /// </summary>
    public static void Fatal(object obj) => Logger.Fatal(obj);

    /// <summary>
    /// Adds a log entry.
    /// The message template uses square brackets for variables. I.e.
    /// <code>"The value of myField is [Value]"</code>
    /// template strings can still use string interpolation. I.e.
    /// <code>$"The value of {myField.Name} is [Value]"</code>
    /// substituted template variables have to be passed using the
    /// <paramref name="args "/>object array
    /// </summary>
    /// <param name="level">the log severity</param>
    /// <param name="template">the message template</param>
    /// <param name="args">substitution parameters</param>
    /// <param name="isPure">logs the template as an unformatted string</param>
    /// <example>Template: $"The value of {myField.Name} is [Value]"</example>
    public static void Log(LogLevel level, string template, object[] args, bool isPure) =>
        Logger.Log(level, template, args, isPure);
}
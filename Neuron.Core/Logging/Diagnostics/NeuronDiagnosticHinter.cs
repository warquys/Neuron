using System;
using System.Collections.Generic;

namespace Neuron.Core.Logging.Diagnostics;

public static class NeuronDiagnosticHinter
{
    public static Dictionary<Type, Func<Exception, string>> ExeptionInformation = new();

    static NeuronDiagnosticHinter()
    {
        AddExeptionInformation<NullReferenceException>(
                    "A NullReferenceException generally means you are trying to " +
                    "access an object, which currently doesn't exist. Make sure to check " +
                    "nullable variables and fields your are using.");
        AddExeptionInformation<IndexOutOfRangeException>(
                    "A IndexOutOfRangeException generally means you are trying to " +
                    "access an index (position) inside of an array or list, that is not " +
                    "within the bounds of the collection. I.e. You are trying to access the " +
                    "3. item in an list that has just 2 items.");
    }

    public static void AddExeptionInformation<T>(string information) where T : Exception
    {
        if (ExeptionInformation.ContainsKey(typeof(T)))
            return;
        ExeptionInformation.Add(typeof(T), (_) => information);
    }

    public static void AddExeptionInformation<T>(Func<Exception, string> information) where T : Exception
    {
        if (ExeptionInformation.ContainsKey(typeof(T)))
            return;
        ExeptionInformation.Add(typeof(T), information);
    }

    public static void AddExeptionInformationHints(Exception exception, DiagnosticsError error)
    {
        var key = exception.GetType();
        if (!ExeptionInformation.ContainsKey(key)) return;
        string message;
        try
        {
            message = ExeptionInformation[key](exception);
        }
        catch (Exception) 
        {
            return;
        }

        error.Nodes.Add(DiagnosticsError.Hint(message));
    }
}
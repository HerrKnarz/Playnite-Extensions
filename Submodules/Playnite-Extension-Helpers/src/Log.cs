using Playnite;
using System.Diagnostics;
using System.Reflection;

namespace PlayniteExtensionHelpers;

/// <summary>
/// Unifies writing messages into the extension.log
/// </summary>
public static class Log
{
    /// <summary>
    /// Logger instance from playnite
    /// </summary>
    private static readonly ILogger _logger = LogManager.GetLogger();

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log</param>
    public static void Debug(string message) => _logger.Debug($"{message}");

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="ex">The exception to log</param>
    /// <param name="message">
    /// Optional message. If the message is empty the calling function will be added to the error message
    /// </param>
    /// <param name="showDialog">Additionally shows the error message as a dialog if set to true.</param>
    /// <param name="api">
    /// Playnite API instance to show the dialog. Can be null if showDialog is false.
    /// </param>
    public static void Error(Exception ex, string message = "", bool showDialog = false, IPlayniteApi? api = null)
    {
        var traceInfos = new TraceInfos(ex);

        if (message.IsNullOrEmpty() && !traceInfos.InitialCaller.IsNullOrEmpty())
        {
            message = $"Error on {traceInfos.InitialCaller}()";
        }

        message += $"|{traceInfos.FileName}|{traceInfos.LineNumber}";

        _logger.Error(ex, $"{message}");

        if (showDialog)
        {
            api?.Dialogs.ShowErrorMessageAsync(message);
        }
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="showDialog">Additionally shows the error message as a dialog if set to true.</param>
    /// <param name="api">
    /// Playnite API instance to show the dialog. Can be null if showDialog is false.
    /// </param>
    public static void Info(string message, bool showDialog = false, IPlayniteApi? api = null)
    {
        _logger.Info($"{message}");

        if (showDialog)
        {
            api?.Dialogs.ShowMessageAsync(message);
        }
    }
}

/// <summary>
/// Retrieves the trace infos from an exception.
/// </summary>
public class TraceInfos
{
    /// <summary>
    /// Retrieves the trace infos from an exception.
    /// </summary>
    /// <param name="ex">Exception to process</param>
    public TraceInfos(Exception ex)
    {
        var trace = new StackTrace(ex, true);
        var frame = trace.GetFrames()?.LastOrDefault();
        InitialCaller = frame?.GetMethod()?.Name;
        CallerParams = frame?.GetMethod()?.GetParameters();
        FileName = frame?.GetFileName().IsNullOrEmpty() ?? true ? "???" : frame.GetFileName();
        LineNumber = frame?.GetFileLineNumber() ?? 0;
    }

    /// <summary>
    /// Parameters of the caller function
    /// </summary>
    public ParameterInfo[]? CallerParams { get; set; }

    /// <summary>
    /// Name of the file the error occurred in.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// The Calling function of the exception
    /// </summary>
    public string? InitialCaller { get; set; }

    /// <summary>
    /// Number of the line the error occurred in.
    /// </summary>
    public int LineNumber { get; set; }
}
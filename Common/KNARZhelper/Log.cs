using Playnite.SDK;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace KNARZhelper
{
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
        /// Logs an error message.
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="message">Optional message. If the message is empty the calling function will be added to the error message</param>
        /// <param name="showDialog">Additionally shows the error message as a dialog if set to true.</param>
        public static void Error(Exception ex, string message = "", bool showDialog = false)
        {
            TraceInfos traceInfos = new TraceInfos(ex);

            if (string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(traceInfos.InitialCaller))
            {
                message = $"Error on {traceInfos.InitialCaller}()";
            }

            message += $"|{traceInfos.FileName}|{traceInfos.LineNumber}";

            _logger.Error(ex, $"{message}");

            if (showDialog)
            {
                API.Instance.Dialogs.ShowErrorMessage(message);
            }
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="showDialog">Additionally shows the error message as a dialog if set to true.</param>
        public static void Info(string message, bool showDialog = false)
        {
            _logger.Info($"{message}");

            if (showDialog)
            {
                API.Instance.Dialogs.ShowMessage(message);
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Debug(string message) => _logger.Debug($"{message}");
    }

    /// <summary>
    /// Retrieves the trace infos from an exception.
    /// </summary>
    public class TraceInfos
    {
        /// <summary>
        /// The Calling function of the exception
        /// </summary>
        public string InitialCaller { get; set; }
        /// <summary>
        /// Parameters of the caller function
        /// </summary>
        public ParameterInfo[] CallerParams { get; set; }
        /// <summary>
        /// Name of the file the error occurred in.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Number of the line the error occurred in.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Retrieves the trace infos from an exception.
        /// </summary>
        /// <param name="ex">Exception to process</param>
        public TraceInfos(Exception ex)
        {
            StackTrace trace = new StackTrace(ex, true);
            StackFrame frame = trace.GetFrames()?.LastOrDefault();
            InitialCaller = frame?.GetMethod()?.Name;
            CallerParams = frame?.GetMethod()?.GetParameters();
            FileName = string.IsNullOrEmpty(frame?.GetFileName()) ? "???" : frame.GetFileName();
            LineNumber = frame?.GetFileLineNumber() ?? 0;
        }
    }
}

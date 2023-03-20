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
        private static readonly ILogger logger = LogManager.GetLogger();

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="Message">Optional message. If the message is empty the calling function will be added to the error message</param>
        /// <param name="showDialog">Additionally shows the error message as a dialog if set to true.</param>
        public static void Error(Exception ex, string Message = "", bool showDialog = false)
        {
            TraceInfos traceInfos = new TraceInfos(ex);

            if (string.IsNullOrEmpty(Message) && !string.IsNullOrEmpty(traceInfos.InitialCaller))
            {
                Message = $"Error on {traceInfos.InitialCaller}()";
            }

            Message += $"|{traceInfos.FileName}|{traceInfos.LineNumber}";

            logger.Error(ex, $"{Message}");

            if (showDialog)
            {
                API.Instance.Dialogs.ShowErrorMessage(Message);
            }
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="Message">The Message to log</param>
        /// <param name="showDialog">Additionally shows the error message as a dialog if set to true.</param>
        public static void Info(string Message, bool showDialog = false)
        {
            logger.Info($"{Message}");

            if (showDialog)
            {
                API.Instance.Dialogs.ShowMessage(Message);
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="Message">The Message to log</param>
        public static void Debug(string Message) => logger.Debug($"{Message}");
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
            StackTrace Trace = new StackTrace(ex, true);
            StackFrame Frame = Trace.GetFrames()?.LastOrDefault();
            InitialCaller = Frame?.GetMethod()?.Name;
            CallerParams = Frame?.GetMethod()?.GetParameters();
            FileName = string.IsNullOrEmpty(Frame.GetFileName()) ? "???" : Frame.GetFileName();
            LineNumber = Frame.GetFileLineNumber();
        }
    }
}

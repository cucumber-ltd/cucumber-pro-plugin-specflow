using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class Logger : ILogger
    {
        private const TraceLevel DefaultLogLevel = TraceLevel.Info;

        private readonly ITraceListener _traceListener;
        private readonly string _logFile;
        public TraceLevel Level { get; }

        public Logger(ITraceListener traceListener, Config config) :
            this(traceListener, GetLogLevel(config), GetLogFile(config))
        {
        }

        internal Logger(ITraceListener traceListener, TraceLevel level, string logFile = null)
        {
            _traceListener = traceListener;
            _logFile = logFile;
            Level = level;

            if (_logFile != null)
                WriteToLogFile("Log initialized", false);
        }

        public void Log(TraceLevel messageLevel, string message)
        {
            if (messageLevel <= Level)
            {
                _traceListener.WriteToolOutput(message);
                WriteToLogFile(message);
            }
        }

        private void WriteToLogFile(string message, bool append = true)
        {
            if (_logFile != null)
            {
                try
                {
                    if (append)
                        File.AppendAllText(_logFile, message + Environment.NewLine, Encoding.UTF8);
                    else
                        File.WriteAllText(_logFile, message + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex, "Error writing to the log file");
                }
            }
        }

        private static string GetLogFile(Config config)
        {
            if (config.IsNull(ConfigKeys.CUCUMBERPRO_LOGFILE))
                return null;

            var fileName = Environment.ExpandEnvironmentVariables(config.GetString(ConfigKeys.CUCUMBERPRO_LOGFILE));
            var assemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ??
                                 Directory.GetCurrentDirectory(); // in the very rare case the assembly folder cannot be detected, we use current directory

            return Path.Combine(assemblyFolder, fileName);
        }

        private static TraceLevel GetLogLevel(Config config)
        {
            if (config.IsNull(ConfigKeys.CUCUMBERPRO_LOGGING))
                return DefaultLogLevel;
            switch (config.GetString(ConfigKeys.CUCUMBERPRO_LOGGING).ToLowerInvariant())
            {
                case "debug":
                case "verbose":
                    return TraceLevel.Verbose;
                case "info":
                    return TraceLevel.Info;
                case "warn":
                case "warning":
                    return TraceLevel.Warning;
                case "error":
                case "fatal":
                    return TraceLevel.Error;
                case "off":
                    return TraceLevel.Off;
            }
            return DefaultLogLevel;
        }
    }
}

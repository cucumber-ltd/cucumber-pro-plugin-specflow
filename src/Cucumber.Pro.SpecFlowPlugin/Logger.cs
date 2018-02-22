using System.Diagnostics;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using TechTalk.SpecFlow.Tracing;

namespace Cucumber.Pro.SpecFlowPlugin
{
    public class Logger : ILogger
    {
        private const TraceLevel DefaultLogLevel = TraceLevel.Info;

        private readonly ITraceListener _traceListener;
        public TraceLevel Level { get; }

        public Logger(ITraceListener traceListener, Config config) :
            this(traceListener, GetLogLevel(config))
        {
        }

        internal Logger(ITraceListener traceListener, TraceLevel level)
        {
            _traceListener = traceListener;
            Level = level;
        }

        public void Log(TraceLevel messageLevel, string message)
        {
            if (messageLevel <= Level)
                _traceListener.WriteToolOutput(message);
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

using System.Diagnostics;

namespace Cucumber.Pro.SpecFlowPlugin
{
    public interface ILogger
    {
        TraceLevel Level { get; }
        void Log(TraceLevel messageLevel, string message);
    }
}

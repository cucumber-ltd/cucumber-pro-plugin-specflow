using System.Collections.Generic;

namespace Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings
{
    public interface IEnvironmentVariablesProvider
    {
        IDictionary<string, string> GetEnvironmentVariables();
    }
}
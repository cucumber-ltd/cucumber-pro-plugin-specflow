using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders
{
    public class EnvironmentVariablesConfigLoader : IConfigLoader
    {
        private readonly IDictionary<string, string> _variables;

        public EnvironmentVariablesConfigLoader() : this(EnvHelper.GetEnvironmentVariables())
        {
        }

        public EnvironmentVariablesConfigLoader(IDictionary<string, string> variables)
        {
            _variables = variables;
        }

        public void Load(Config config)
        {
            foreach (var variable in _variables)
            {
                config.Set(variable.Key, variable.Value);
            }
        }
    }
}

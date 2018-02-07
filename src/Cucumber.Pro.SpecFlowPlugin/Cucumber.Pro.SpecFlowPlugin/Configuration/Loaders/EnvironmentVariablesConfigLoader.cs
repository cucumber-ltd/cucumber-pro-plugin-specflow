using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders
{
    public class EnvironmentVariablesConfigLoader : IConfigLoader
    {
        private readonly Dictionary<string, string> _variables;

        public EnvironmentVariablesConfigLoader() : this(Environment.GetEnvironmentVariables()
            .OfType<DictionaryEntry>()
            .ToDictionary(i => i.Key.ToString(), i => i.Value?.ToString()))
        {
        }

        public EnvironmentVariablesConfigLoader(Dictionary<string, string> variables)
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

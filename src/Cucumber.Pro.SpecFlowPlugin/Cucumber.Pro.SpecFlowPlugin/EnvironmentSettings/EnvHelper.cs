using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings
{
    public static class EnvHelper
    {
        public static IDictionary<string, string> GetEnvironmentVariables() =>
            Environment.GetEnvironmentVariables()
                .OfType<DictionaryEntry>()
                .ToDictionary(i => i.Key.ToString(), i => i.Value?.ToString());
    }
}

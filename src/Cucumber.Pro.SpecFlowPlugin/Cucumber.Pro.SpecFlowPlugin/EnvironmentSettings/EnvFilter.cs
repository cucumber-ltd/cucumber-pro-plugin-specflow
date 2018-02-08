using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cucumber.Pro.SpecFlowPlugin.Configuration;

namespace Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings
{
    public class EnvFilter
    {
        private readonly Regex _maskPatternRe;

        public EnvFilter(Config config)
        {
            var mask = config.GetString(ConfigKeys.CUCUMBERPRO_ENVMASK);
            _maskPatternRe = new Regex($".*({mask}).*", RegexOptions.IgnoreCase);
        }

        public IDictionary<string, string> Filter(IDictionary<string, string> env)
        {
            return env.Where(e => !_maskPatternRe.Match(e.Key).Success)
                .ToDictionary(e => e.Key, e => e.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders
{
    public class YamlConfigLoader : IConfigLoader
    {
        private readonly YamlStream _yaml;

        public YamlConfigLoader(TextReader reader)
        {
            _yaml = new YamlStream();
            _yaml.Load(reader);
        }

        public void Load(Config config)
        {
            Populate(config, (YamlMappingNode)_yaml.Documents[0].RootNode);
        }

        private void Populate(Config config, YamlMappingNode mapping)
        {
            foreach (var entry in mapping.Children)
            {
                var property = ((YamlScalarNode) entry.Key).Value.Replace("_", "");
                var value = entry.Value;
                if (value is YamlScalarNode && string.IsNullOrWhiteSpace(((YamlScalarNode)value).Value))
                    config.SetNull(property);
                else if (value is YamlScalarNode)
                {
                    config.Set(property, ((YamlScalarNode) value).Value);
                }
                else if (value is YamlMappingNode)
                {
                    var childConfig = config.GetChild(property);
                    Populate(childConfig, (YamlMappingNode) value);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}

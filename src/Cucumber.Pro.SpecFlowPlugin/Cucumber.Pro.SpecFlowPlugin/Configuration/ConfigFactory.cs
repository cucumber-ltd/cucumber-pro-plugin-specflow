using System;
using System.IO;
using Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public class ConfigFactory
    {
        public static readonly string[] GLOBAL_YAML_FILE_NAMES = {
            "/usr/local/etc/cucumber/cucumber.yml",
            Environment.GetEnvironmentVariable("HOMEPATH") + "/.cucumber.yml",
        };

        public static readonly string[] LOCAL_YAML_FILE_NAMES = {
            "cucumber.yml",
            ".cucumber.yml",
            ".cucumber/cucumber.yml",
            ".cucumber/cucumber.yml",
            ".cucumberpro.yml"
        };


        private static void LoadYamlConfigFiles(string[] filePaths, Config config)
        {
            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    using (var reader = File.OpenText(filePath))
                    {
                        var loader = new YamlConfigLoader(reader);
                        loader.Load(config);
                    }
                }
            }
        }

        public static Config Create()
        {
            var config = ConfigDefaults.CreateConfig();

            // The order is defined by "globalness". The principle is to make it easy
            // to define global values, but equally easy to override them on a per-project
            // basis.
            LoadYamlConfigFiles(GLOBAL_YAML_FILE_NAMES, config);
            new EnvironmentVariablesConfigLoader().Load(config);
            //new BambooEnvironmentVariablesConfigLoader().Load(config);
            //new DeprecatedEnvironmentVariablesConfigLoader().Load(config);
            //new SystemPropertiesConfigLoader().Load(config);
            LoadYamlConfigFiles(LOCAL_YAML_FILE_NAMES, config);

            return config;
        }
    }
}

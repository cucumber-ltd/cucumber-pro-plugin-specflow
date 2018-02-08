using System;
using System.IO;
using Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public class ConfigFactory
    {
        public const string CONFIG_FILE_NAME = ".cucumberpro.yml";

        public static readonly string[] GLOBAL_YAML_FILE_NAMES = {
            Path.Combine(Environment.GetEnvironmentVariable("HOMEPATH"), CONFIG_FILE_NAME)
        };

        public static readonly string[] LOCAL_YAML_FILE_NAMES = {
            Path.Combine("..", "..", CONFIG_FILE_NAME),
            Path.Combine("..", CONFIG_FILE_NAME),
            CONFIG_FILE_NAME
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

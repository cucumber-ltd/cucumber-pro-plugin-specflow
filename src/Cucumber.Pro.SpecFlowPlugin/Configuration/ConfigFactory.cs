﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cucumber.Pro.SpecFlowPlugin.Configuration.Loaders;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public class ConfigFactory
    {
        public static readonly string[] CONFIG_FILE_NAMES = {
            "C:\\cucumber\\cucumber.yml",
            Path.Combine(Environment.SpecialFolder.Personal.ToString(), ".cucumber", "cucumber.yml"),
            Path.Combine(Environment.SpecialFolder.Personal.ToString(), "cucumber.yml"),
            "cucumber.yml",
            Path.Combine(".cucumber", "cucumber.yml")
        };

        public static readonly string[] GLOBAL_YAML_FOLDERS = {
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        private static IEnumerable<string> GetFoldersUp()
        {
            var testFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ??
                             Directory.GetCurrentDirectory();

            var directory = new DirectoryInfo(testFolder);
            while (directory != null)
            {
                yield return directory.FullName;
                directory = directory.Parent;
            }
        }

        private static void LoadYamlConfigFiles(IEnumerable<string> folders, Config config)
        {
            foreach (var folder in folders)
            {
                if (string.IsNullOrEmpty(folder))
                    continue;

                foreach (var fileName in CONFIG_FILE_NAMES)
                {
                    var filePath = Path.GetFullPath(Path.Combine(folder, fileName));
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
        }

        public static Config Create()
        {
            var config = ConfigKeys.CreateDefaultConfig();

            // The order is defined by "globalness". The principle is to make it easy
            // to define global values, but equally easy to override them on a per-project
            // basis.
            LoadYamlConfigFiles(GLOBAL_YAML_FOLDERS, config);
            LoadYamlConfigFiles(GetFoldersUp(), config);
            new EnvironmentVariablesConfigLoader().Load(config);

            return config;
        }
    }
}

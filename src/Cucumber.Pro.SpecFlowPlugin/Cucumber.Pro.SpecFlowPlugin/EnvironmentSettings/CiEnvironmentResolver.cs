using System;
using System.Collections.Generic;
using Cucumber.Pro.SpecFlowPlugin.Configuration;

namespace Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings
{
    public class CiEnvironmentResolver
    {
        private readonly string _revision;
        private readonly string _branch;

        public CiEnvironmentResolver(string revision, string branch)
        {
            _revision = revision;
            _branch = branch;
        }

        public void Resolve(Config config)
        {
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_REVISION, _revision);
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_BRANCH, _branch);
        }

        private void SetIfNotSet(Config config, string key, string value)
        {
            if (value != null && config.IsNull(key))
                config.Set(key, value);
        }

        public static CiEnvironmentResolver Detect(IDictionary<string, string> env)
        {
            return
                DetectBamboo(env) ??
                DetectCircle(env) ??
                DetectJenkins(env) ??
                DetectTravis(env) ??
                CreateLocal(env);
        }

        private static Tuple<string, string> GetEnvValues(IDictionary<string, string> env, string revisionKey, string branchKey)
        {
            if (!env.ContainsKey(revisionKey))
                return null;

            return new Tuple<string, string>(env[revisionKey], env[branchKey]);
        }

        private static CiEnvironmentResolver DetectBamboo(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env, "bamboo_planRepository_revision", "bamboo_repository_git_branch");
            return vaules == null ? null : new CiEnvironmentResolver(vaules.Item1, vaules.Item2);
        }

        private static CiEnvironmentResolver DetectCircle(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env, "CIRCLE_SHA1", "CIRCLE_BRANCH");
            return vaules == null ? null : new CiEnvironmentResolver(vaules.Item1, vaules.Item2);
        }

        private static CiEnvironmentResolver DetectJenkins(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env, "GIT_COMMIT", "GIT_BRANCH");
            return vaules == null ? null : new CiEnvironmentResolver(vaules.Item1, vaules.Item2);
        }

        private static CiEnvironmentResolver DetectTravis(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env, "TRAVIS_COMMIT", "TRAVIS_BRANCH");
            return vaules == null ? null : new CiEnvironmentResolver(vaules.Item1, vaules.Item2);
        }

        private static CiEnvironmentResolver CreateLocal(IDictionary<string, string> env)
        {
            return new CiEnvironmentResolver("local" + DateTime.Now.ToString("yyyyMMddhhmmss"), null);
        }
    }
}

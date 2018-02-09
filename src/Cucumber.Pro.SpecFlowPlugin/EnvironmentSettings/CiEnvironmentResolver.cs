using System;
using System.Collections.Generic;
using System.Linq;
using Cucumber.Pro.SpecFlowPlugin.Configuration;

namespace Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings
{
    public class CiEnvironmentResolver
    {
        private readonly string _revision;
        private readonly string _branch;
        private readonly string _projectName;

        public CiEnvironmentResolver(string revision, string branch, string projectName)
        {
            _revision = revision;
            _branch = branch;
            _projectName = projectName;
        }

        public void Resolve(Config config)
        {
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_REVISION, _revision);
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_BRANCH, _branch);
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_PROJECTNAME, _projectName);
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

        private static Tuple<string, string, string> GetEnvValues(IDictionary<string, string> env, string revisionKey, string branchKey, string projectNameKey)
        {
            if (!env.ContainsKey(revisionKey))
                return null;

            return new Tuple<string, string, string>(env[revisionKey], env[branchKey], projectNameKey == null ? null : env[projectNameKey]);
        }

        // https://confluence.atlassian.com/bamboo/bamboo-variables-289277087.html
        private static CiEnvironmentResolver DetectBamboo(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "bamboo_planRepository_revision",
                "bamboo_repository_git_branch",
                "bamboo_planRepository_name");
            return vaules == null ? null : new CiEnvironmentResolver(vaules.Item1, vaules.Item2, vaules.Item3);
        }

        // https://circleci.com/docs/2.0/env-vars/#circleci-environment-variable-descriptions
        private static CiEnvironmentResolver DetectCircle(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "CIRCLE_SHA1",
                "CIRCLE_BRANCH",
                "CIRCLE_PROJECT_REPONAME");
            return vaules == null ? null : new CiEnvironmentResolver(vaules.Item1, vaules.Item2, vaules.Item3);
        }

        private static CiEnvironmentResolver DetectJenkins(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "GIT_COMMIT",
                "GIT_BRANCH",
                null); // no option for resolving project name?
            return vaules == null ? null : new CiEnvironmentResolver(vaules.Item1, vaules.Item2, vaules.Item3);
        }

        // https://docs.travis-ci.com/user/environment-variables/#Default-Environment-Variables
        private static CiEnvironmentResolver DetectTravis(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "TRAVIS_COMMIT",
                "TRAVIS_BRANCH",
                "TRAVIS_REPO_SLUG");
            if (vaules == null)
                return null;
            var projectRepoSlug = vaules.Item3;
            var projectName = projectRepoSlug?.Split('/').Last();
            return new CiEnvironmentResolver(vaules.Item1, vaules.Item2, projectName);
        }

        private static CiEnvironmentResolver CreateLocal(IDictionary<string, string> env)
        {
            return new CiEnvironmentResolver(
                "local" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                null,
                null);
        }
    }
}

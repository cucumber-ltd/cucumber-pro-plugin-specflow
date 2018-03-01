using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cucumber.Pro.SpecFlowPlugin.Configuration;

namespace Cucumber.Pro.SpecFlowPlugin.EnvironmentSettings
{
    public class CiEnvironmentResolver
    {
        private readonly string _ciName;
        private readonly string _revision;
        private readonly string _branch;
        private readonly string _projectName;
        private readonly string _repositoryRoot;
        private readonly string _tag;

        private CiEnvironmentResolver(string ciName, Tuple<string, string, string, string, string> values)
            : this(ciName, values.Item1, values.Item2, values.Item3, values.Item4, values.Item5)
        {
        }

        public CiEnvironmentResolver(string ciName, string revision, string branch, string projectName, string repositoryRoot, string tag)
        {
            _revision = revision;
            _branch = branch;
            _projectName = projectName;
            _ciName = ciName;
            _repositoryRoot = repositoryRoot;
            _tag = tag;
        }

        public bool IsDetected => _ciName != null;
        public string CiName => _ciName ?? "Local";

        public void Resolve(Config config)
        {
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_GIT_REVISION, _revision);
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_GIT_BRANCH, _branch);
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_GIT_TAG, _tag);
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_PROJECTNAME, _projectName);
            SetIfNotSet(config, ConfigKeys.CUCUMBERPRO_GIT_REPOSITORYROOT, _repositoryRoot ?? DetectRepositoryRoot());
        }

        private string DetectRepositoryRoot()
        {
            var testFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            if (testFolder == null)
                return null;
            var directory = new DirectoryInfo(testFolder);
            while (directory != null)
            {
                if (directory.GetDirectories(".git").Any())
                    return directory.FullName;

                directory = directory.Parent;
            }
            return null;
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
                DetectTfs(env) ??
                DetectTravis(env) ??
                DetectWercker(env) ??
                DetectLocal(env) ??
                CreateUnknown(env);
        }

        private static string GetOrNull(IDictionary<string, string> env, string key)
        {
            return key == null || !env.TryGetValue(key, out var value) ? null : value;
        }

        private static Tuple<string, string, string, string, string> GetEnvValues(IDictionary<string, string> env, string revisionKey, string branchKey, string projectNameKey, string repositoryRootKey, string tagKey)
        {
            if (!env.ContainsKey(revisionKey))
                return null;

            return new Tuple<string, string, string, string, string>(
                GetOrNull(env, revisionKey),
                GetOrNull(env, branchKey),
                GetOrNull(env, projectNameKey),
                GetOrNull(env, repositoryRootKey),
                GetOrNull(env, tagKey));
        }

        // https://confluence.atlassian.com/bamboo/bamboo-variables-289277087.html
        private static CiEnvironmentResolver DetectBamboo(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "bamboo_planRepository_revision",
                "bamboo_repository_git_branch",
                "bamboo_planRepository_name",
                "bamboo_build_working_directory", // repo root
                null); // tag
            return vaules == null ? null : new CiEnvironmentResolver("Bamboo", vaules);
        }

        // https://circleci.com/docs/2.0/env-vars/#circleci-environment-variable-descriptions
        private static CiEnvironmentResolver DetectCircle(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "CIRCLE_SHA1",
                "CIRCLE_BRANCH",
                "CIRCLE_PROJECT_REPONAME",
                "CIRCLE_WORKING_DIRECTORY", // repo root
                "CIRCLE_TAG");
            return vaules == null ? null : new CiEnvironmentResolver("Circle", vaules);
        }

        // https://wiki.jenkins.io/display/JENKINS/Git+Plugin#GitPlugin-Environmentvariables
        // https://wiki.jenkins.io/display/JENKINS/Git+Tag+Message+Plugin
        private static CiEnvironmentResolver DetectJenkins(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "GIT_COMMIT",
                "GIT_BRANCH",
                null, // no option for resolving project name?
                null, // repo root
                "GIT_TAG_NAME"); 
            return vaules == null ? null : new CiEnvironmentResolver("Jenkins", vaules);
        }

        // https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?tabs=batch#predefined-variables
        private static CiEnvironmentResolver DetectTfs(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "BUILD_SOURCEVERSION",
                "BUILD_SOURCEBRANCHNAME",
                "SYSTEM_TEAMPROJECT",
                "BUILD_REPOSITORY_LOCALPATH",
                null); // tag name
            return vaules == null ? null : new CiEnvironmentResolver("TFS", vaules);
        }

        // https://docs.travis-ci.com/user/environment-variables/#Default-Environment-Variables
        private static CiEnvironmentResolver DetectTravis(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "TRAVIS_COMMIT",
                "TRAVIS_BRANCH",
                "TRAVIS_REPO_SLUG",
                "TRAVIS_BUILD_DIR", // repo root
                "TRAVIS_TAG"); // tag name
            if (vaules == null)
                return null;
            var projectRepoSlug = vaules.Item3;
            var projectName = projectRepoSlug?.Split('/').Last();
            return new CiEnvironmentResolver("Travis", vaules.Item1, vaules.Item2, projectName, vaules.Item4, vaules.Item5);
        }

        // http://devcenter.wercker.com/docs/environment-variables/available-env-vars
        private static CiEnvironmentResolver DetectWercker(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "WERCKER_GIT_COMMIT",
                "WERCKER_GIT_BRANCH",
                "TRAVIS_REPO_SLUG",
                "TRAVIS_BUILD_DIR", // repo root
                "TRAVIS_TAG"); // tag name
            if (vaules == null)
                return null;
            var projectRepoSlug = vaules.Item3;
            var projectName = projectRepoSlug?.Split('/').Last();
            return new CiEnvironmentResolver("Travis", vaules.Item1, vaules.Item2, projectName, vaules.Item4, vaules.Item5);
        }

        private static CiEnvironmentResolver DetectLocal(IDictionary<string, string> env)
        {
            var vaules = GetEnvValues(env,
                "GIT_COMMIT",
                "GIT_BRANCH",
                null, //project name
                null, // repo root
                "GIT_TAG");
            return vaules == null ? null : new CiEnvironmentResolver("Local", vaules);
        }

        private static CiEnvironmentResolver CreateUnknown(IDictionary<string, string> env)
        {
            return new CiEnvironmentResolver(null, "local" + DateTime.Now.ToString("yyyyMMddhhmmss"), null, null, null, null);
        }
    }
}

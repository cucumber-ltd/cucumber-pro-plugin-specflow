using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cucumber.Pro.SpecFlowPlugin.Publishing;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Publishing
{
    public class HttpMultipartResultsPublisherTests
    {
        public class CProStubNancyModule : NancyModule
        {
            public static bool IsInvoked;
            public static string ProjectName;
            public static string Revision;
            public static string ProfileName;
            public static string Json;
            public static Dictionary<string, string> Env;

            public CProStubNancyModule()
            {
                Post["{projectName}/{revision}"] = parameters =>
                {
                    IsInvoked = true;

                    ProjectName = parameters.projectName;
                    Revision = parameters.revision;

                    ProfileName = Request.Form["profileName"];

                    var envStream = Request.Files.First(f => f.Key.Contains("env")).Value;
                    var envContent = new StreamReader(envStream).ReadToEnd();
                    Env = envContent.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToDictionary(l => l.Split(new[] { '=' }, 2).First(), l => l.Split(new[] { '=' }, 2).Last());

                    var jsonStream = Request.Files.First(f => f.Key.Contains("payload")).Value;
                    Json = new StreamReader(jsonStream).ReadToEnd();

                    return 200;
                };
            }
        }

        [Fact]
        public void posts_results_as_multipart_formadata()
        {
            var tempFile = Path.GetTempFileName();
            var json = @"[{ ""name"": ""Eating cucumbers""}]";
            File.WriteAllText(tempFile, json);
            var env = new Dictionary<string, string> { { "env1", "value1" }, { "env2", "value2" } };
            var profileName = "my-profile";

            var hostConfiguration = new HostConfiguration {RewriteLocalhost = false};
            using (var nancyHost = new NancyHost(new Uri("http://localhost:8082/tests/results/"), NancyBootstrapperLocator.Bootstrapper, hostConfiguration))
            {
                nancyHost.Start();

                var publisher = new HttpMultipartResultsPublisher("http://localhost:8082/tests/results/prj/rev1");
                publisher.PublishResults(tempFile, env, profileName);
            }

            Assert.True(CProStubNancyModule.IsInvoked);
            Assert.Equal("prj", CProStubNancyModule.ProjectName);
            Assert.Equal("rev1", CProStubNancyModule.Revision);
            Assert.Equal(profileName, CProStubNancyModule.ProfileName);
            Assert.Equal(env, CProStubNancyModule.Env);
            Assert.Equal(json, CProStubNancyModule.Json);
        }
    }
}

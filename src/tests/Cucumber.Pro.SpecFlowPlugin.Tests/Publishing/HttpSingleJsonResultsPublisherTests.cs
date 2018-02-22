using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.Publishing;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Xunit;
using Xunit.Abstractions;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Publishing
{
    public class HttpSingleJsonResultsPublisherTests
    {
        public class CProStubNancyModule : NancyModule
        {
            public static bool IsInvoked;
            public static string ProjectName;
            public static string Json;
            public static string Auth;
            public static int ExpectedResponseCode = 200;
            public static int WaitMilliseconds = 0;

            public static string NormalizedJson => Json == null ? null : Regex.Replace(Json, @"\s+", "");

            public static void Reset(int expectedResponseCode = 200, int waitMilliseconds = 0)
            {
                IsInvoked = false;
                ProjectName = null;
                Json = null;
                Auth = null;
                ExpectedResponseCode = expectedResponseCode;
                WaitMilliseconds = waitMilliseconds;
            }

            public CProStubNancyModule()
            {
                Post["{projectName}"] = parameters =>
                {
                    IsInvoked = true;

                    if (WaitMilliseconds > 0)
                        System.Threading.Thread.Sleep(WaitMilliseconds);

                    Auth = Request.Headers.Authorization;

                    ProjectName = parameters.projectName;

                    var jsonStream = Request.Body;
                    Json = new StreamReader(jsonStream).ReadToEnd();

                    return ExpectedResponseCode;
                };
            }
        }

        const string SampleJson = @"[{""name"":""Eating_cucumbers"",""elements"":[]}]"; // no whitespaces!
        static readonly Dictionary<string, string> SampleEnv = new Dictionary<string, string> { { "env1", "value1" }, { "env2", "value2" } };
        const string SampleProfileName = "my-profile";
        const string SampleToken = "my-token";
        private const string SampleProjectName = "prj";
        private const string SampleBranch = "master";
        private const string SampleRevision = "rev1";
        private readonly StubTraceListener stubTraceListener;

        public HttpSingleJsonResultsPublisherTests(ITestOutputHelper testOutputHelper)
        {
            stubTraceListener = new StubTraceListener(testOutputHelper);
        }

        const string SampleUrl = "http://localhost:8082/tests/results/" + SampleProjectName;

        private void PublishResultsToStub(Func<HttpSingleJsonResultsPublisher> publisherFactory = null, int timeout = 5000, bool checkInvoked = true)
        {
            var hostConfiguration = new HostConfiguration {RewriteLocalhost = false};
            using (var nancyHost = new NancyHost(new Uri("http://localhost:8082/tests/results/"),
                NancyBootstrapperLocator.Bootstrapper, hostConfiguration))
            {
                nancyHost.Start();

                var publisher = publisherFactory != null ? publisherFactory() :
                    new HttpSingleJsonResultsPublisher(stubTraceListener.Logger, url: SampleUrl, token: SampleToken, timeoutMilliseconds: timeout, revision: SampleRevision, branch: SampleBranch, tag: null);
                publisher.PublishResultsFromContent(SampleJson, SampleEnv, SampleProfileName);
            }

            if (checkInvoked)
                Assert.True(CProStubNancyModule.IsInvoked);
        }

        [Fact]
        public void Posts_feature_results_as_part_of_the_json()
        {
            CProStubNancyModule.Reset();
            PublishResultsToStub();
            Assert.Equal(SampleProjectName, CProStubNancyModule.ProjectName);
            AssertFeatureResultsInJson();
        }

        private static void AssertFeatureResultsInJson()
        {
            Assert.Contains(SampleJson, CProStubNancyModule.NormalizedJson);
        }

        [Fact]
        public void Posts_ENV_as_part_of_the_json()
        {
            CProStubNancyModule.Reset();
            PublishResultsToStub();
            Assert.Equal(SampleProjectName, CProStubNancyModule.ProjectName);
            AssertSampleEnvInJson();
        }

        private static void AssertSampleEnvInJson()
        {
            foreach (var env in SampleEnv)
            {
                Assert.Contains($"\"{env.Key}\":\"{env.Value}\"", CProStubNancyModule.NormalizedJson);
            }
        }

        [Fact]
        public void Posts_Revision_as_part_of_the_json()
        {
            CProStubNancyModule.Reset();
            PublishResultsToStub();
            Assert.Equal(SampleProjectName, CProStubNancyModule.ProjectName);
            AssertRevisionInJson();
        }

        private static void AssertRevisionInJson()
        {
            Assert.Contains(SampleRevision, CProStubNancyModule.NormalizedJson);
        }

        [Fact]
        public void Posts_ProfileName_as_part_of_the_json()
        {
            CProStubNancyModule.Reset();
            PublishResultsToStub();
            Assert.Equal(SampleProjectName, CProStubNancyModule.ProjectName);
            AssertProfileNameInJson();
        }

        private static void AssertProfileNameInJson()
        {
            Assert.Contains(SampleProfileName, CProStubNancyModule.NormalizedJson);
        }

        [Fact]
        public void Sets_token_as_basic_auth()
        {
            CProStubNancyModule.Reset();
            PublishResultsToStub();
            var expectedAuth = $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(SampleToken + ":"))}";
            Assert.Equal(expectedAuth, CProStubNancyModule.Auth);
        }

        [Fact]
        public void Logs_success_message()
        {
            CProStubNancyModule.Reset();
            PublishResultsToStub();
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("Cucumber Pro"));
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains(SampleUrl));
        }

        [Fact]
        public void Logs_missing_auth()
        {
            CProStubNancyModule.Reset(401);
            PublishResultsToStub();
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("Failed"));
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains(ConfigKeys.CUCUMBERPRO_TOKEN));
        }

        [Fact]
        public void Logs_wrong_auth()
        {
            CProStubNancyModule.Reset(403);
            PublishResultsToStub();
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("Failed"));
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains(ConfigKeys.CUCUMBERPRO_TOKEN));
        }

        [Fact]
        public void Logs_other_error()
        {
            CProStubNancyModule.Reset(500);
            PublishResultsToStub();
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("Failed"));
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("500"));
        }

        [Fact]
        public void Handles_timeout()
        {
            CProStubNancyModule.Reset(200, waitMilliseconds: 100);
            PublishResultsToStub(timeout: 5, checkInvoked: false);
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("Cucumber Pro"));
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("timed out"));
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains(ConfigKeys.CUCUMBERPRO_CONNECTION_TIMEOUT));
        }

        [Fact]
        public void Can_read_from_config()
        {
            var config = ConfigKeys.CreateDefaultConfig();
            config.Set(ConfigKeys.CUCUMBERPRO_URL, "http://localhost:8082/");
            config.Set(ConfigKeys.CUCUMBERPRO_PROJECTNAME, SampleProjectName);
            config.Set(ConfigKeys.CUCUMBERPRO_GIT_REVISION, SampleRevision);
            config.Set(ConfigKeys.CUCUMBERPRO_TOKEN, SampleToken);
            config.Set(ConfigKeys.CUCUMBERPRO_CONNECTION_TIMEOUT, 5000);

            CProStubNancyModule.Reset();
            PublishResultsToStub(() => new HttpSingleJsonResultsPublisher(config, stubTraceListener.Logger));
            Assert.Equal(SampleProjectName, CProStubNancyModule.ProjectName);
            AssertProfileNameInJson();
            AssertSampleEnvInJson();
            AssertFeatureResultsInJson();
            AssertRevisionInJson();
        }
    }
}

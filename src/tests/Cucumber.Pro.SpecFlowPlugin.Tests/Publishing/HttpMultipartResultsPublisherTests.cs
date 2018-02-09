using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Cucumber.Pro.SpecFlowPlugin.Publishing;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using TechTalk.SpecFlow.Tracing;
using Xunit;
using Xunit.Abstractions;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Publishing
{
    public class HttpMultipartResultsPublisherTests
    {
        public class StubTraceListener : ITraceListener
        {
            private ITestOutputHelper _testOutputHelper;

            public StubTraceListener(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            public List<string> TestOutput { get; } = new List<string>();
            public List<string> ToolOutput { get; } = new List<string>();

            public void WriteTestOutput(string message)
            {
                _testOutputHelper.WriteLine(message);
                TestOutput.Add(message);
            }

            public void WriteToolOutput(string message)
            {
                _testOutputHelper.WriteLine("> " + message);
                ToolOutput.Add(message);
            }
        }

        public class CProStubNancyModule : NancyModule
        {
            public static bool IsInvoked;
            public static string ProjectName;
            public static string Revision;
            public static string ProfileName;
            public static string Json;
            public static string Auth;
            public static Dictionary<string, string> Env;
            public static int ExpectedResponseCode = 200;
            public static int WaitMilliseconds = 0;

            public static void Reset(int expectedResponseCode = 200, int waitMilliseconds = 0)
            {
                IsInvoked = false;
                ProjectName = null;
                Revision = null;
                ProfileName = null;
                Json = null;
                Auth = null;
                Env = null;
                ExpectedResponseCode = expectedResponseCode;
                WaitMilliseconds = waitMilliseconds;
            }

            public CProStubNancyModule()
            {
                Post["{projectName}/{revision}"] = parameters =>
                {
                    IsInvoked = true;

                    if (WaitMilliseconds > 0)
                        System.Threading.Thread.Sleep(WaitMilliseconds);

                    Auth = Request.Headers.Authorization;

                    ProjectName = parameters.projectName;
                    Revision = parameters.revision;

                    ProfileName = Request.Form["profileName"];

                    var envStream = Request.Files.First(f => f.Key.Contains("env")).Value;
                    var envContent = new StreamReader(envStream).ReadToEnd();
                    Env = envContent.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToDictionary(l => l.Split(new[] { '=' }, 2).First(), l => l.Split(new[] { '=' }, 2).Last());

                    var jsonStream = Request.Files.First(f => f.Key.Contains("payload")).Value;
                    Json = new StreamReader(jsonStream).ReadToEnd();

                    return ExpectedResponseCode;
                };
            }
        }

        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;

        const string SampleJson = @"[{ ""name"": ""Eating cucumbers""}]";
        static readonly Dictionary<string, string> SampleEnv = new Dictionary<string, string> { { "env1", "value1" }, { "env2", "value2" } };
        const string SampleProfileName = "my-profile";
        const string SampleToken = "my-token";
        private readonly StubTraceListener stubTraceListener;

        public HttpMultipartResultsPublisherTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            stubTraceListener = new StubTraceListener(_testOutputHelper);
        }

        const string SampleUrl = "http://localhost:8082/tests/results/prj/rev1";

        private void PublishResultsToStub(int timeout = 5000, bool checkInvoked = true)
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, SampleJson);

            var hostConfiguration = new HostConfiguration {RewriteLocalhost = false};
            using (var nancyHost = new NancyHost(new Uri("http://localhost:8082/tests/results/"),
                NancyBootstrapperLocator.Bootstrapper, hostConfiguration))
            {
                nancyHost.Start();

                var publisher = new HttpMultipartResultsPublisher(SampleUrl, SampleToken, stubTraceListener, timeout);
                publisher.PublishResults(tempFile, SampleEnv, SampleProfileName);
            }

            if (checkInvoked)
                Assert.True(CProStubNancyModule.IsInvoked);
        }

        [Fact]
        public void Posts_results_as_multipart_formadata()
        {
            CProStubNancyModule.Reset();
            PublishResultsToStub();
            Assert.Equal("prj", CProStubNancyModule.ProjectName);
            Assert.Equal("rev1", CProStubNancyModule.Revision);
            Assert.Equal(SampleProfileName, CProStubNancyModule.ProfileName);
            Assert.Equal(SampleEnv, CProStubNancyModule.Env);
            Assert.Equal(SampleJson, CProStubNancyModule.Json);
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
            PublishResultsToStub(5, checkInvoked: false);
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("Cucumber Pro"));
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains("timed out"));
            Assert.Contains(stubTraceListener.ToolOutput, msg => msg.Contains(ConfigKeys.CUCUMBERPRO_CONNECTION_TIMEOUT));
        }
    }
}

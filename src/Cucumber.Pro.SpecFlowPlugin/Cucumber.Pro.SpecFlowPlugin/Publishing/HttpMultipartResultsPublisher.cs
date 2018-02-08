using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public class HttpMultipartResultsPublisher : IResultsPublisher
    {
        public void PublishResults(string resultsJsonFilePath, IDictionary<string, string> env, string profileName)
        {
            var resultsJson = File.ReadAllText(resultsJsonFilePath);

            var httpClient = new HttpClient();
            //httpClient.BaseAddress = new Uri("https://app.cucumber.pro/tests/results/SpecSol_Test1");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("fe3e1a5f27789a139a963ff56cddb00816c" + ":")));
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(@"cucumber_pro_build_reference=build123
cucumber_pro_git_branch=master
".Replace("\r\n", "\n")), "env", "env.txt");
            content.Add(new StringContent("default"), profileName);
            content.Add(new StringContent(resultsJson, Encoding.UTF8,
                "application/x.cucumber.java.results+json"), "payload", "results.json");
            var response = httpClient
                .PostAsync("https://app.cucumber.pro/tests/results/SpecSol_Test1/master", content).Result;
            Console.WriteLine(response);
        }
    }
}

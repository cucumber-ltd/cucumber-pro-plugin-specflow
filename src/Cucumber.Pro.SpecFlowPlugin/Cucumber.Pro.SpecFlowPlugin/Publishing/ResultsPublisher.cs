﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public class ResultsPublisher
    {
        public void PublishResults(string resultsJsonFilePath)
        {
            var resultsJson = File.ReadAllText(resultsJsonFilePath);

            var httpClient = new HttpClient();
            //httpClient.BaseAddress = new Uri("https://app.cucumber.pro/tests/results/SpecSol_Test1");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("fe3e1a5f27789a139a963ff56cddb00816c" + ":")));
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("env1=envval"), "env", "env.txt");
            content.Add(new StringContent("default"), "profileName");
            content.Add(new StringContent(resultsJson, Encoding.UTF8,
                "application/x.cucumber.java.results+json"), "payload", "results.json");
            var response = httpClient
                .PostAsync("https://app.cucumber.pro/tests/results/SpecSol_Test1/master", content).Result;
            Console.WriteLine(response);
        }
    }
}

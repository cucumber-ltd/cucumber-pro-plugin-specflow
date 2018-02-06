using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.Publishing
{
    public class ResultsPublisher
    {
        public void PublishResults(string resultsJsonFilePath)
        {
            var resultsJson = File.ReadAllText(resultsJsonFilePath);

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://fe3e1a5f27789a139a963ff56cddb00816c@app.cucumber.pro/tests/results/SpecSol_Test1");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("envvars"), "env");
            content.Add(new StringContent("default"), "profileName");
            //content.Add(new StringContent(resultsJson, Encoding.UTF8, "application/x.specflow.results+json"), "payload");
            content.Add(new StringContent(resultsJson, Encoding.UTF8, "application/x.cucumber.java.results+json"), "payload");
            var response = httpClient.PostAsync("master", content).Result;
            Console.WriteLine(response);
        }
    }
}

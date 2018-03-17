using Cucumber.Pro.SpecFlowPlugin.Publishing;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Publishing
{
    public class CucumberProResultsUrlBuilderTests
    {
        [Fact]
        public void Uri_encodes_arguments()
        {
            var url = CucumberProResultsUrlBuilder.BuildCucumberProUrl("https://app.cucumber.pro/", "/ !'()~?");
            Assert.Equal("https://app.cucumber.pro/tests/results/%2F%20!'()~%3F", url);
        }
    }
}

using System;
using TechTalk.SpecFlow;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.SampleProject.StepDefinitions
{
    public class Belly
    {
        public int EatenCucumbers { get; private set; }

        public Belly(int cucumbers)
        {
            EatenCucumbers = cucumbers;
        }

        public void Eat(int cucumbers)
        {
            EatenCucumbers += cucumbers;
        }
    }

    [Binding]
    public class EatingCucumberSteps
    {
        private Belly _belly;

        [Given(@"I have already eaten (.*) cucumbers")]
        public void GivenIHaveAlreadyEatenCucumbers(int cucumbers)
        {
            _belly = new Belly(cucumbers);
        }
        
        [When(@"I eat (.*) cucumbers")]
        public void WhenIEatCucumbers(int cucumbers)
        {
            _belly.Eat(cucumbers);
        }
        
        [Then(@"I should have (.*) cucumbers in my belly")]
        public void ThenIShouldHaveCucumbersInMyBelly(int expectedCount)
        {
            Assert.Equal(expectedCount, _belly.EatenCucumbers);
        }

        [When(@"there is a pending step")]
        public void WhenThereIsAPendingStep()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"there is an error")]
        [BeforeScenario("beforescenario_error")]
        [AfterScenario("afterscenario_error")]
        public void WhenThereIsAnError()
        {
            throw new Exception("simulated error");
        }

        [Then(@"the last step is skipped")]
        public void ThenTheLastStepIsSkipped()
        {
        }
    }
}

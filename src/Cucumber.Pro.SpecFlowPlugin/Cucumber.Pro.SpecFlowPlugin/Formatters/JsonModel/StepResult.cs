namespace Cucumber.Pro.SpecFlowPlugin.Formatters.JsonModel
{
    public class StepResult
    {
        public int Line { get; set; } //???
        public string Keyword { get; set; }
        public string Name { get; set; }
        public Result Result { get; set; }
    }
}

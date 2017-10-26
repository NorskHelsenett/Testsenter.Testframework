using TestFramework.Behave;
using Shared.Common.DI;
using TestFramework.Resources.Attributes;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [InitWebDriver]
    [BehavePatternStep("Open Browser at (.*)")]
    public class OpenBrowser : BaseExecuteTestStep
    {
        public const string DriverKey = "driver";

        public OpenBrowser(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            var url = GetTestData(1);

            WebDriver.Navigate().GoToUrl(url);

            return TestStepResult.Successful();
        }

        public override void Cleanup(TestStepResult result)
        {
        }
    }
}

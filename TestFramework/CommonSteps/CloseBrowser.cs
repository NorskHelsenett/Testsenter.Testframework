using TestFramework.Behave;
using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Close Browser")]
    public class CloseBrowser : BaseExecuteTestStep
    {
        public CloseBrowser(IDependencyInjector injector) : base(injector)
        {

        }

        public override TestStepResult Do()
        {
            var driver = (IWebDriver)TestState.GetInstanceWithKey("driver");
            driver.Dispose();

            return TestStepResult.Successful();
        }
    }
}

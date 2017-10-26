using TestFramework.Behave;
using TestFramework.Core;
using TestFramework.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Oppdater nettleser")]
    public class RefreshBrowser : BaseExecuteTestStep
    {
        public RefreshBrowser(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            WebDriver.Navigate().Refresh();
            WebDriver.WaitForAjaxCall(5);
            return TestStepResult.Successful();
        }
    }
}

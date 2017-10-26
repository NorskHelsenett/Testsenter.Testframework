using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Behave;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Click link with text (.*)")]
    public class ClickLinkWithText : BaseExecuteTestStep
    {
        private int _wait = 10;
        public const string DriverKey = "driver";

        public ClickLinkWithText(IDependencyInjector injector) : base(injector)
        {

        }

        public override TestStepResult Do()
        {
            var linkText = GetTestData(1);

            var el = WebDriver.FindElement(By.LinkText(linkText));
            el.Click();

            return TestStepResult.Successful();
        }

        public override void Cleanup(TestStepResult result)
        {
        }

    }
}

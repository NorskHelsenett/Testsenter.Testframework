using TestFramework.Selenium;
using TestFramework.Behave;
using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Check that element (.*) with selector (.*) has text (.*) in view")]
    class CheckElementHasText : BaseExecuteTestStep
    {
        public CheckElementHasText(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            var driver = (IWebDriver)TestState.GetInstanceWithKey("driver");
            var elementQuery = GetTestData(1);
            var selectorType = GetTestData(2);
            var text = GetTestData(3);

            var element = driver.FindElement(SeleniumUtil.GetSelector(selectorType, elementQuery));

            if (!element.Text.Contains(text))
            {
                return TestStepResult.Failed($"Element did not contain the expected text. Expected: '{text}' Got: '{element.Text}'");
            }

            return TestStepResult.Successful();
        }
    }
}

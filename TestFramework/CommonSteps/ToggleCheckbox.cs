using TestFramework.Selenium;
using TestFramework.Behave;
using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Toggle Checkbox element (.*) with selector (.*)")]
    class ToggleCheckbox : BaseExecuteTestStep
    {
        public ToggleCheckbox(IDependencyInjector injector) : base(injector)
        {

        }

        public override TestStepResult Do()
        {
            var driver = (IWebDriver)TestState.GetInstanceWithKey("driver");
            var elementQuery = GetTestData(1);
            var selectorType = GetTestData(2);

            var element = driver.FindElement(SeleniumUtil.GetSelector(selectorType, elementQuery));
            if (null == element)
            {
                return TestStepResult.Failed($"Could not find Element {elementQuery}");
            }
            element.Click();

            return TestStepResult.Successful();
        }
    }
}

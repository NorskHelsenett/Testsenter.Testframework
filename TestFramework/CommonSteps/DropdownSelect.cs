using TestFramework.Behave;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TestFramework.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Select dropdown item by text (.*) in element (.*) with selector (.*)")]
    class DropdownSelect : BaseExecuteTestStep
    {
        public DropdownSelect(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            var driver = (IWebDriver)TestState.GetInstanceWithKey("driver");

            var text = GetTestData(1);
            var elementQuery = GetTestData(2);
            var selectorType = GetTestData(3);

            var select = new SelectElement(driver.FindElement(SeleniumUtil.GetSelector(selectorType, elementQuery)));
            
            select.SelectByText(text);

            return TestStepResult.Successful();
        }
    }
}

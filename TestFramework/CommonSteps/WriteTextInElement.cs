using OpenQA.Selenium;
using TestFramework.Behave;
using TestFramework.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Write text (.*) into element (.*) with selector (.*)")]
    public class WriteTextInElement : BaseExecuteTestStep
    {
        public WriteTextInElement(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            var driver = (IWebDriver)TestState.GetInstanceWithKey("driver");
            var text = GetTestData(1);
            var elementQuery = GetTestData(2);
            var selectorType = GetTestData(3);


            var element = driver.FindElement(SeleniumUtil.GetSelector(selectorType, elementQuery));
            if (null == element)
            {
                return TestStepResult.Failed($"Could not find Element {elementQuery}");
            }
            element.SendKeys(text);

            return TestStepResult.Successful();
        }
    }
}

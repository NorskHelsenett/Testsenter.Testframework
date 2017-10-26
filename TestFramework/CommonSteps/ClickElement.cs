using System;
using System.Threading;
using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Behave;
using TestFramework.Core;
using TestFramework.Resources;
using TestFramework.Selenium;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Click element (.*) with selector (.*)")]
    public class ClickElement : BaseExecuteTestStep
    {
        public ClickElement(IDependencyInjector injector) : base(injector)
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
            var dsn = element.GetAttribute("data-service-name");

            driver.SwitchTo().Window(driver.CurrentWindowHandle);
            driver.FindElement(By.XPath("//body")).Click();

            if (element.GetAttribute("href") != null)
            {
                element.Click();
                Thread.Sleep(5000);
            }
            else
            {
                element.Click();
            }

            return TestStepResult.Successful();
        }
    }

    
}

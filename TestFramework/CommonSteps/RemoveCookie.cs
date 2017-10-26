using TestFramework.Behave;
using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Delete Cookie name (.*)")]
    public class RemoveCookie : BaseExecuteTestStep
    {
        public RemoveCookie(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            var driver = (IWebDriver)TestState.GetInstanceWithKey("driver");

            var cookieName = GetTestData(1);

            var cookies = driver.Manage().Cookies;
            if (cookies.GetCookieNamed(cookieName) != null)
            {
                cookies.DeleteCookieNamed(cookieName);
            }

            return TestStepResult.Successful();
        }
    }
}

using TestFramework.Behave;
using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Set Cookie name (.*) to value (.*)")]
    public class SetCookie : BaseExecuteTestStep
    {
        public SetCookie(IDependencyInjector injector) : base(injector)
        {

        }

        public override TestStepResult Do()
        {
            var driver = (IWebDriver)TestState.GetInstanceWithKey("driver");

            var cookieName = GetTestData(1);
            var cookieValue = GetTestData(2);

            var cookies = driver.Manage().Cookies;
            if (cookies.GetCookieNamed(cookieName) != null)
            {
                cookies.DeleteCookieNamed(cookieName);
            }
            cookies.AddCookie(new Cookie(cookieName, cookieValue));

            return TestStepResult.Successful();
        }
    }
}

using TestFramework.Behave;
using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Check that cookie exists with name (.*)")]
    public class HasCookie : BaseExecuteTestStep
    {
        public HasCookie(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            var driver = (IWebDriver)TestState.GetInstanceWithKey("driver");

            var cookieName = GetTestData(1);

            var cookies = driver.Manage().Cookies;
            if (cookies.GetCookieNamed(cookieName) == null)
            {
                TestStepResult.Failed($"Failed to find cookie {cookieName}");
            }

            return TestStepResult.Successful();
        }
    }
}

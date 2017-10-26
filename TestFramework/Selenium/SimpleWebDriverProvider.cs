using System.Threading.Tasks;
using log4net;
using OpenQA.Selenium;
using TestFramework.Interfaces;

namespace TestFramework.Selenium
{
    public class SimpleWebDriverProvider : IWebDriverProvider
    {
        private readonly PreferredSeleniumBrowser _preferredBrowser;

        public SimpleWebDriverProvider(PreferredSeleniumBrowser browser)
        {
            _preferredBrowser = browser;
        }

        public void Cleanup()
        {
        }

        public async Task<IWebDriver> Get(string processId)
        {
            return SeleniumSetup.GetWebDriver(_preferredBrowser);
        }

        public void Release(IWebDriver webDriver, bool cleanupSucceeded)
        {
            webDriver.Quit();
        }
    }
}

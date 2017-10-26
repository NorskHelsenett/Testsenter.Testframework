using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFramework.Selenium;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shared.Common.Log;

namespace Test
{
    [TestClass]
    public class WebDriverProviderTests
    {
        [TestMethod]
        public void WebDriverProvider_ProvidesOneWebDriver()
        {
            var webDriverProvider = new WebDriverProvider(10, 10, PreferredSeleniumBrowser.Chrome, new DummyLog());

            var webDriver = webDriverProvider.Get("Testdriver").Result;

            Assert.IsInstanceOfType(webDriver, typeof(IWebDriver));

            webDriver.Quit();
        }

        [TestMethod]
        public void WebDriverProvider_ProvidesMaxWebDrivers()
        {
            var maxDrivers = 10;
            var webDriverProvider = new WebDriverProvider(maxDrivers, 10, PreferredSeleniumBrowser.PhantomJS, new DummyLog());
            var drivers = new List<IWebDriver>();

            Parallel.For(0, 10, (i) =>
            {
                var webDriver = webDriverProvider.Get("Testdriver").Result;
                drivers.Add(webDriver);

                Assert.IsInstanceOfType(webDriver, typeof(IWebDriver));
            });

            Assert.IsTrue(webDriverProvider.GetNumberOfRunningDrivers() <= maxDrivers);

            foreach (var driver in drivers)
            {
                driver.Quit();
            }
        }

        [TestMethod]
        public void WebDriverProvider_ProvidesMoreThanMaxWebDrivers()
        {
            var maxDrivers = 2;
            var webDriverProvider = new WebDriverProvider(maxDrivers, 10, PreferredSeleniumBrowser.PhantomJS, new DummyLog());
            var usedDrivers = new List<IWebDriver>();
            var numberOfTasksThatGotDriver = 0;
            var numberOfTasksToRequestDriver = 20;

            var tasks = new List<Task>();

            for(var i = 0; i < numberOfTasksToRequestDriver; i++)
            {
                tasks.Add(Task.Run(() => 
                {
                    var webDriver = webDriverProvider.Get("Testdriver").Result;
                    usedDrivers.Add(webDriver);
                    numberOfTasksThatGotDriver++;

                    Task.Delay(1000).Wait();

                    Assert.IsInstanceOfType(webDriver, typeof(IWebDriver));

                    webDriverProvider.Release(webDriver, true);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.IsTrue(webDriverProvider.GetNumberOfRunningDrivers() <= maxDrivers);
            Assert.AreEqual(numberOfTasksToRequestDriver, numberOfTasksThatGotDriver);

            foreach (var driver in usedDrivers)
            {
                driver.Quit();
            }
        }
    }
}

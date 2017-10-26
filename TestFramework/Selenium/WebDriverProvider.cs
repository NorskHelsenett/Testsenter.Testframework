using OpenQA.Selenium;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using TestFramework.Interfaces;

namespace TestFramework.Selenium
{
    public class WebDriverProvider : IWebDriverProvider
    {
        private readonly List<PooledWebDriver> _drivers;
        private readonly ConcurrentDictionary<Guid, DateTime> _claimedDrivers;
        private readonly int _maxMinutesToKeepClaim;
        private bool _stopCleanupTask;
        private readonly PreferredSeleniumBrowser _preferredBrowser;
        private readonly ILog _logger;
        private readonly Task _idleWebDriverCleaner;

        public WebDriverProvider(int maxNumberOfDrivers, int maxMinutesToKeepClaim, PreferredSeleniumBrowser browser, ILog logger)
        {
            _maxMinutesToKeepClaim = maxMinutesToKeepClaim;
            _claimedDrivers = new ConcurrentDictionary<Guid, DateTime>();
            _preferredBrowser = browser;
            _logger = logger;
            _stopCleanupTask = false;

            _idleWebDriverCleaner = Task.Run(async () =>
            {
                while (!_stopCleanupTask)
                {
                    await Task.Delay(10000);

                    var removeThese = new List<Guid>();

                    foreach (var webdriver in _claimedDrivers)
                    {
                        var claimedOverdue = DateTime.UtcNow.Subtract(webdriver.Value).TotalMinutes > _maxMinutesToKeepClaim;
                        if (!claimedOverdue)
                            continue;

                        var driver = _drivers.FirstOrDefault(g => g.Id == webdriver.Key);
                        if (driver != null)
                        {
                            _logger.Warn("Releasing webdriver " + driver.Id + " due to inactivity beyond " + _maxMinutesToKeepClaim + " minutes");
                            Release(driver, false);
                        }
                        else
                        {
                            removeThese.Add(webdriver.Key);
                        }
                    }

                    foreach (var remove in removeThese)
                    {
                        DateTime timestamp;
                        bool removed = false;
                        do
                        {
                            removed = _claimedDrivers.TryRemove(remove, out timestamp);
                        }
                        while (!removed);
                    }
                }
            });

            _drivers = new List<PooledWebDriver>();
            for (int i=0; i<maxNumberOfDrivers; i++)
            {
                _drivers.Add(new PooledWebDriver());
            }
        }

        public void Cleanup()
        {
            foreach(var driver in _drivers)
            {
                if (driver.WebDriver != null)
                    driver.WebDriver.Quit();
            }

            _stopCleanupTask = true;
            _idleWebDriverCleaner.Wait();
        }

        public async Task<IWebDriver> Get(string processId)
        {
            return await WaitForAvailable(processId);
        }

        public void Release(IWebDriver webDriver, bool cleanupSucceeded)
        {
            var pooledDriver = _drivers.First(d => d.WebDriver == webDriver);
            Release(pooledDriver, cleanupSucceeded);
        }

        private void Release(PooledWebDriver pooledDriver, bool cleanupSucceeded)
        {
            DateTime timestamp;

            if (!cleanupSucceeded)
            {
                pooledDriver.WebDriver?.Quit();
                pooledDriver.IsInitialized = false;
            }

            bool removed = false;
            do
            {
                removed = _claimedDrivers.TryRemove(pooledDriver.Id, out timestamp);
            }
            while (!removed);
        }

        public int GetNumberOfRunningDrivers()
        {
            return _drivers.Count;
        }

        private async Task<IWebDriver> WaitForAvailable(string processId)
        {
            var stopwa = new Stopwatch();
            stopwa.Start();

            IWebDriver claimedDriver = null;
            while (claimedDriver == null)
            {
                var firstAvailable = _drivers.FirstOrDefault(d => !_claimedDrivers.ContainsKey(d.Id));

                if (firstAvailable != null)
                {
                    var added = _claimedDrivers.TryAdd(firstAvailable.Id, DateTime.UtcNow);
                    if (added)
                    {
                        bool cont = true;
                        if (!firstAvailable.IsInitialized)
                        {
                            try
                            {
                                firstAvailable.WebDriver = SeleniumSetup.GetWebDriver(_preferredBrowser);
                            }
                            catch (Exception)
                            {
                                DateTime timestamp;
                                firstAvailable.IsInitialized = false;
                                _claimedDrivers.TryRemove(firstAvailable.Id, out timestamp);
                                cont = false;
                            }
                        }

                        if (cont)
                        {
                            firstAvailable.IsInitialized = true;
                            claimedDriver = firstAvailable.WebDriver;
                            break;
                        }
                    }
                }
                else
                {
                    _logger.Info("Process with id " + processId + " still waiting for webdriver to claim. Waited for " + stopwa.Elapsed.TotalMinutes + " minutes");
                    await Task.Delay(2000);
                }
            }

            return claimedDriver;
        }

        
    }

    public class PooledWebDriver
    {
        public Guid Id { get; set; }
        public IWebDriver WebDriver { get; set; }

        public bool IsInitialized { get; set; }


        public PooledWebDriver()
        {
            Id = Guid.NewGuid();
            IsInitialized = false;
        }
    }
}

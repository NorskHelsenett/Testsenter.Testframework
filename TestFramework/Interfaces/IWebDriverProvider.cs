using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace TestFramework.Interfaces
{
    public interface IWebDriverProvider
    {
        Task<IWebDriver> Get(string processId);
        void Release(IWebDriver webDriver, bool cleanupSucceeded);
        void Cleanup();
    }
}

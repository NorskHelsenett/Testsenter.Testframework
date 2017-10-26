using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace TestFramework.Selenium
{
    public class SeleniumSetup
    {
        public static IWebDriver GetWebDriver(PreferredSeleniumBrowser preferredBrowser)
        {
            IWebDriver webDriver = null;
            bool success = false;
            int tries = 10;
            while(!success)
            {
                webDriver = StartWebDriver(preferredBrowser);
                success = HealthCheck(webDriver, throwException: (tries-- <= 0));
                if(!success)
                {
                    try
                    {
                        webDriver.Quit();
                    }
                    catch (Exception e)
                    {
                        var x = 2;
                    }
                }
            }

            return webDriver;
        }

        private static IWebDriver StartWebDriver(PreferredSeleniumBrowser preferredBrowser)
        {
            switch (preferredBrowser)
            {
                case PreferredSeleniumBrowser.Chrome:
                    return GetChromeDriver(headless: false);
                case PreferredSeleniumBrowser.ChromeHeadless:
                    return GetChromeDriver(headless: true);
                case PreferredSeleniumBrowser.FireFox:
                    return GetFirefoxDriver();
                case PreferredSeleniumBrowser.PhantomJS:
                    return GetPhantomJSDriver();
                default:
                    return StartWebDriver(DefaultBrowser());
            }
        }

        private static bool HealthCheck(IWebDriver webDriver, bool throwException)
        {
            try
            {
                webDriver.Manage().Window.Size = new Size(1920, 1080);
                webDriver.Navigate().Refresh();
                return true;
            }
            catch(Exception)
            {
                if (throwException)
                    throw;

                return false;
            }
        }

        public static IWebDriver GetChromeDriver(bool headless)
        {
            var options = new ChromeOptions();
            if (headless) options.AddArguments("--headless", "--disable-gpu"); // does not launch a GUI
            else options.AddArgument("--start-maximized"); // ensures that the screen is maximized ...

            var driver = new ChromeDriver(SeleniumDriversDirectory, options);
            return driver;
        }

        public static IWebDriver GetFirefoxDriver()
        {
            var defaultBrowserPath = GetDefaultBrowserPath();
            defaultBrowserPath = defaultBrowserPath.Replace("\" -osint", "");

            return new FirefoxDriver(new FirefoxBinary(defaultBrowserPath), new FirefoxProfile(), TimeSpan.FromMinutes(10));
        }

        private static PhantomJSDriver GetPhantomJSDriver()
        {
            var defService = PhantomJSDriverService.CreateDefaultService(SeleniumDriversDirectory);
            defService.IgnoreSslErrors = true;
            defService.SslProtocol = "any";
            defService.WebSecurity = false;
            defService.DiskCache = false;

            var driver = new PhantomJSDriver(defService, new PhantomJSOptions(), new TimeSpan(0, 0, 34));
            return driver;
        }

        protected static PreferredSeleniumBrowser DefaultBrowser()
        {
            var defaultBrowserPath = GetDefaultBrowserPath();
            return defaultBrowserPath.Contains("firefox") 
                ? PreferredSeleniumBrowser.FireFox 
                : PreferredSeleniumBrowser.Chrome;
        }

        public static string GetDefaultBrowserPath()
        {
            string urlAssociation = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http";
            string browserPathKey = @"$BROWSER$\shell\open\command";

            RegistryKey userChoiceKey = null;
            string browserPath = "";

            try
            {
                //Read default browser path from userChoiceLKey
                userChoiceKey = Registry.CurrentUser.OpenSubKey(urlAssociation + @"\UserChoice", false);

                //If user choice was not found, try machine default
                if (userChoiceKey == null)
                {
                    //Read default browser path from Win XP registry key
                    var browserKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                    //If browser path wasn’t found, try Win Vista (and newer) registry key
                    if (browserKey == null)
                    {
                        browserKey =
                        Registry.CurrentUser.OpenSubKey(
                        urlAssociation, false);
                    }
                    var path = CleanifyBrowserPath(browserKey.GetValue(null) as string);
                    browserKey.Close();
                    return path;
                }
                else
                {
                    // user defined browser choice was found
                    string progId = (userChoiceKey.GetValue("ProgId").ToString());
                    userChoiceKey.Close();

                    // now look up the path of the executable
                    string concreteBrowserKey = browserPathKey.Replace("$BROWSER$", progId);
                    var kp = Registry.ClassesRoot.OpenSubKey(concreteBrowserKey, false);
                    browserPath = CleanifyBrowserPath(kp.GetValue(null) as string);
                    kp.Close();
                    return browserPath;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get browser path - {ex.Message}", ex);
            }
        }

        private static string CleanifyBrowserPath(string uglyPath)
        {
            //return uglyPath.Split('\"')[0];
            return uglyPath.Substring(1, uglyPath.Length - 10).TrimEnd('\\');
        }

        public static string SeleniumDriversDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path) + "/Selenium/Drivers";
            }
        }
    }
}

using OpenQA.Selenium;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;
using OpenQA.Selenium.PhantomJS;
using TestFramework.Resources;

namespace TestFramework.Selenium
{
    public static class SeleniumUtil
    {
        #region JQuery
        public static string JQuery(this IWebDriver webDriver, string jquery)
        {
            var text = RunScript(webDriver, "return " + jquery);
            return text as string;
        }

        public static T JQuery<T>(this IWebDriver webDriver, string jquery)
        {
            var obj = RunScript(webDriver, "return " + jquery);
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static IWebElement JQueryFindElement(this IWebDriver webDriver, string jquery)
        {
            var elements = JQueryFindElements(webDriver, jquery);

            return elements.First();
        }

        public static List<IWebElement> JQueryFindElements(this IWebDriver webDriver, string jquery)
        {
            var found = webDriver.RunScript("return " + jquery);
            var elements = new List<IWebElement>();

            foreach (IWebElement webElement in (IEnumerable)found)
            {
                elements.Add(webElement);
            }

            return elements;
        }
        
        public static void JQueryVal(this IWebDriver webDriver, string jquerySelector, string val, bool appendJqueryQuote = true)
        {
            string script = appendJqueryQuote ?
                 "$(\"" + jquerySelector + "\").val(\"" + val + "\");" :
                 jquerySelector + ".val(\"" + val + "\");";

            RunScript(webDriver, script);
        }
        
        public static void JQueryClick(this IWebDriver webDriver, string jquery, bool appendJqueryQuote = true)
        {
            string script = appendJqueryQuote ?
                 "$(\"" + jquery + "\").click();" :
                 jquery + ".click();";

            RunScript(webDriver, script);
        }

        public static void JQueryInListClick(this IWebDriver webDriver, string innerJquerySelector, int index)
        {
            string script = "$('" + innerJquerySelector + "')[" + index + "].click();";
            RunScript(webDriver, script);
        }
        #endregion

        public static bool HasClass(this IWebElement webElement, string className)
        {
            return webElement.GetAttribute("class").Split(' ').Contains(className);
        }

        public static IWebElement TryFindElement(this IWebDriver webDriver, By by)
        {
            try
            {
                return webDriver.FindElement(@by);
            }
            catch
            {
                return null;
            }
        }

        public static IWebElement TryFindElement(this IWebElement webElement, By by)
        {
            try
            {
                return webElement.FindElement(@by);
            }
            catch
            {
                return null;
            }
        }

        public static IReadOnlyCollection<IWebElement> TryFindElements(this IWebElement webElement, By by)
        {
            try
            {
                return webElement.FindElements(@by);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> WaitForElementInDocumentAsync(this IWebDriver webDriver, By by, int tries = 10, int delay = 3, string message = null)
        {
            var originalTries = tries;
            while (tries > 0)
            {
                var isVisible = webDriver.TryFindElement(@by) != null;

                if (isVisible)
                    return true;

                await Task.Delay(delay * 1000);

                tries--;
            }

            if (String.IsNullOrEmpty(message))
                throw new TestCaseException($"Waited for {@by} to appear for {originalTries * delay} seconds, but it did not appear.");
            else
                throw new TestCaseException(message);

        }

        public static async Task<bool> WaitForElementVisible(this IWebDriver webDriver, string cssSelector, int tries = 10, int delay = 3, string message = null)
        {
            var originalTries = tries;
            while (tries > 0)
            {
                var isVisible = cssSelector.Contains("'") ? webDriver.JQuery<bool>("$(\"" + cssSelector + "\").is(':visible')") : webDriver.JQuery<bool>("$('" + cssSelector + "').is(':visible')");

                if (isVisible)
                    return true;

                await Task.Delay(delay * 1000);

                tries--;
            }

            if(string.IsNullOrEmpty(message))
                throw new TestCaseException($"Waited for {cssSelector} to appear for {originalTries * delay} seconds, but it did not appear.");
            else
                throw new TestCaseException(message);

        }

        public static async Task<bool> WaitForElementInvisible(this IWebDriver webDriver, string cssSelector, int tries = 10, int delay = 3, string message = null)
        {
            var stillVisible = true;
            var originalTries = tries;

            while (tries > 0)
            {
                stillVisible = webDriver.JQuery<bool>("$('" + cssSelector + "').is(':visible')") || webDriver.JQuery<bool>("$('" + cssSelector + ":visible').length > 0");

                if (!stillVisible)
                    break;

                await Task.Delay(delay * 1000);

                tries--;
            }

            if (stillVisible)
                throw new TestCaseException($"Waited for {cssSelector} to disappear for {originalTries * delay} seconds, but it did not disappear.");

            return true;
        }

        public static void ScrollToTop(this IWebDriver webDriver)
        {
            RunScript(webDriver, "window.scrollTo(0, arguments[0]);", 0);
            Thread.Sleep(500);
        }

        public static void ScrollUntilElementIsInMiddleOfScreen(this IWebDriver webDriver, IWebElement element)
        {
            var offSetY = webDriver.Manage().Window.Size.Height / 2;
            int yPosition = element.Location.Y + ((-1) * offSetY);

            ScrollDown(webDriver, yPosition);
        }

        public static void ScrollDown(this IWebDriver webDriver, int yPosition)
        {
            RunScript(webDriver, "window.scrollTo(0, arguments[0]);", yPosition);
            Thread.Sleep(500);
        }

        public static async Task<IWebElement> WaitForElement(this IWebDriver webDriver, By selector, int numberOfTries = 5)
        {
            Exception e = null;
            while(numberOfTries-- > 0)
            {
                try
                {
                    await Task.Delay(2000);
                    return webDriver.FindElement(selector);
                }
                catch(Exception f)
                {
                    e = f;
                }
            }

            throw e;
        }

        

        public static async Task TryThisAsyncWait(Action action, int numberOfTries, string failmsg = "")
        {
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception) when (numberOfTries-- > 0)
                {
                    await Task.Delay(3000);
                }
            }
        }

        public static async Task<T> WaitForElement<T>(this IWebDriver webDriver, Func<IWebDriver, T> func, int numberOfTries = 5)
        {
            Exception e = null;
            while (numberOfTries-- > 0)
            {
                try
                {
                    await Task.Delay(2000);
                    return func(webDriver);
                }
                catch (Exception f)
                {
                    e = f;
                }
            }

            throw e;
        }

        public static object RunScript(this IWebDriver webDriver, string s, params object[] args)
        {
            IJavaScriptExecutor js = webDriver as IJavaScriptExecutor;
            if (args != null && args.Length > 0)
                return js.ExecuteScript(s, args);
            else
                return js.ExecuteScript(s);
        }

        public static By GetSelector(string selectorType, string element)
        {
            switch (selectorType)
            {
                case "Name":
                    return By.Name(element);
                case "XPath":
                    return By.XPath(element);
                case "CSS":
                    return By.CssSelector(element);
                case "ID":
                    return By.Id(element);
                default:
                    throw new NotImplementedException($"No Selector with name {selectorType}");
            }
        }

        public static void ClickElement(IWebDriver driver, IWebElement elem)
        {
            (driver as IJavaScriptExecutor)?.ExecuteScript($"window.scrollTo(0, {elem.Location.Y});");
            elem.Click();
        }

        public static void WaitForAjaxCall(this IWebDriver webDriver, int secondsToWait = 5)
        {
            Thread.Sleep(secondsToWait * 1000);
        }

        public static IWebElement SelectOption(this IWebElement element, string value)
        {
            return element.FindElement(By.CssSelector($"option[value='{value}'"));
        }

        public static IWebElement FindElementById(this IWebDriver driver, string id)
        {
            return driver.FindElement(By.Id(id));
        }

        public static IWebElement FindElementByTitle(this IWebDriver driver, string title)
        {
            var xPath = FindElementWithinHtmlTagWithAttribute(title, "title");
            return driver.FindElementByXPath(xPath);
        }

        public static void ClickElementWithJavascript(this IWebDriver driver, string id)
        {
            ((IJavaScriptExecutor) driver).ExecuteScript($"document.getElementById('{id}')");
        }

        public static IWebElement FindElementByXPath(this IWebDriver driver, string path)
        {
            return driver.FindElement(By.XPath(path));
        }

        public static IWebElement FindElementByXPath(this IWebElement element, string path)
        {
            //INFO: When you want to find an element within element you have to add . in front
            return element.FindElement(By.XPath("." + path));
        }

        public static IWebElement FindElementByClassName(this IWebDriver driver, string @class)
        {
            return driver.FindElementByXPath(Path(@class));
        }

        public static IWebElement FindElementByClassName(this IWebElement element, string @class)
        {
            return element.FindElementByXPath(Path(@class));
        }

        private static string Path(string @class)
        {
            return  $"//*[contains(@class, ' {@class}') or contains(@class, '{@class} ') or @class='{@class}']";
        }

        public static IWebElement FindElementByCssSelector(this IWebDriver driver, string path)
        {
            return driver.FindElement(By.CssSelector(path));
        }

        public static IEnumerable<IWebElement> FindElementsByXPath(this IWebDriver driver, string path)
        {
            return driver.FindElements(By.XPath(path));
        }

        public static IWebElement FindElementByPartialId(this IWebDriver driver, string id, string htmlTag = null)
        {
            var xPath = FindElementWithinHtmlTagWithAttribute(id, "id", htmlTag);
            return driver.FindElement(By.XPath(xPath));
        }

        public static IEnumerable<IWebElement> FindElementsByPartialId(this IWebDriver driver, string id, string htmlTag = null)
        {
            var xPath = FindElementWithinHtmlTagWithAttribute(id, "id", htmlTag);
            return driver.FindElements(By.XPath(xPath));
        }

        public static IWebElement InputText(
            this IWebElement element,
            string text,
            bool clearInput = true,
            bool clearWithKeyboard = false)
        {
            if (clearInput)
            {
                element.ClearElement(clearWithKeyboard);
            }
            element.SendKeys(text);
            return element;
        }
       
        public static IWebElement WaitUntilVisible(this IWebElement element, TimeSpan? wait = null)
        {
            WaitUntil(() => IsVisible(element), "Element did not become visible", wait);
            return element;
        }

        public static void WaitUntilNotVisible(IWebElement element, TimeSpan? wait = null)
        {
            WaitUntil(() => !IsVisible(element), "Element did not become invisible", wait);
        }

        public static bool IsVisible(this IWebElement element)
        {
            return element.Enabled && element.Displayed;
        }

        public static void WaitUntil(Func<bool> condition, string description, TimeSpan? wait = null)
        {
            wait = wait ?? TimeSpan.FromSeconds(10);
            var end = DateTime.Now + wait;
            while (DateTime.Now < end)
            {
                if (condition.Invoke())
                {
                    return;
                }
            }

            throw new Exception($"{description} in {wait.Value.TotalSeconds} seconds");
        }

        public static IWebElement ScrollToElement(this IWebElement element, IWebDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            return element;
        }

        public static IWebElement ClickElement(this IWebElement element)
        {
            element.Click();
            return element;
        }

        public static void WaitForPageToLoad(this IWebDriver driver)
        {
            WaitUntil(() => ((IJavaScriptExecutor)driver)
                                .ExecuteScript("return document.readyState")
                                .Equals("complete"),
                        "Page did not render");
        }

        public static void PressEnter(this IWebElement element)
        {
            element.SendKeys(Keys.Enter);
        }

        public static IWebElement FindElementByText(this IWebDriver driver, string text)
        {
            return driver.FindElementByXPath($"//*[contains(text(),'{text}')]");
        }

        public static IAlert GetSeleniumAlert(IWebDriver webdriver)
        {
            var driver = webdriver as PhantomJSDriver;
            if (driver != null)
            {
                var js = (IJavaScriptExecutor)webdriver;

                js.ExecuteScript("window.confirm = function(){return true;}");

                driver.ExecutePhantomJS("var page = this;" +
                                               "page.onConfirm = function(msg) {" +
                                               "console.log('CONFIRM: ' + msg);return true;" +
                                                  "};");
                return null;
            }

            try
            {
                return webdriver.SwitchTo().Alert();
            }
            catch (NoAlertPresentException)
            {
                return null;
            }
        }

        public static void Wait(this IWebElement webElement, int secondsToWait)
        {
            Thread.Sleep(secondsToWait * 1000);
        }

        private static string FindElementWithinHtmlTagWithAttribute(string id, string attribute, string parentHtml = null )
        {
            return parentHtml == null
               ? $"//*[contains(@{attribute}, '{id}')]"
               : $"//{parentHtml}[contains(@{attribute}, '{id}')]";
        }

        private static void ClearElement(this IWebElement element, bool clearWithKeyboard)
        {
            if (!clearWithKeyboard)
            {
                element.Clear();
            }
            else
            {
                element.SendKeys(Keys.Control + "a");
                element.SendKeys(Keys.Backspace);
            }
        }

        public static IWebElement[] FindElementsRecursively(this ISearchContext node, params By[] selectors)
        {
            var children = node.FindElements(selectors[0]).ToArray();
            selectors = selectors.Skip(1).ToArray();
            return selectors.Length > 0
                ? children.SelectMany(e => e.FindElementsRecursively(selectors)).ToArray()
                : children;
        }
    }
}


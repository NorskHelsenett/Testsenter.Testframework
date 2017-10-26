using Shared.Common.Resources;
using System;
using Shared.Common.DI;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using TestFramework.Behave;
using TestFramework.Resources;
using TestFramework.Interfaces;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using Shared.Common.Testing;
using TestFramework.Selenium;
using TestFramework.TestHelpers.Attachements;

namespace TestFramework.Core
{
    public class BaseExecuteTestStep : IExecuteTestStep
    {
        #region Properties
        private bool _calledFromBase;

        public TestState TestState { get; set; }

        public TestData TestData { get; set; }

        public TestStep TestStep { get; set; }

        public int BehaveStepId { get; set; }

        protected virtual string Description { get; set; }

        protected readonly IDependencyInjector _injector;
        public IDependencyInjector Injector
        {
            get
            {
                if (_injector == null) throw new Exception("BaseTestExecutor sin IDependencyInjector er ikke satt, bruk constructor BaseTestExecutor(IDependencyInjector injector)");
                return _injector;
            }
        }

        protected ILog Log { get; }

        public IWebDriver WebDriver
        {
            get
            {
                if (TestState.Contains("driver"))
                    return (IWebDriver)TestState.GetInstanceWithKey("driver");

                throw new Exception("Driver is not set in testState");
            }
        }

        #endregion

        protected BaseExecuteTestStep()
        {
        }

        protected BaseExecuteTestStep(IDependencyInjector injector)
        {
            _injector = injector;
            Log = injector.GetInstance<ILog>();
        }

        public void Init() {
            MapBehaviourPatternToTestData();
        }

        

        public string GetTestData(int index)
        {
            List<string> data = GetData(index);
            return data[index].TrimStart('@');
        }

        private List<string> GetData(int index)
        {
            var data = GetMatchedItemsFromPattern(TestStep.Description);
            if (index >= data.Count)
            {
                throw new ArgumentException($"Missing arguments: Asked for TestData[{index}], but was only {data.Count} elements.");
            }

            return data;
        }

        public string GetTestDataOrValueToLower(int index)
        {
            string data = GetData(index)[index];
            string dataCase =  data.Contains("@") ? TestData.GetValue(data.TrimStart('@')) : data;
            return dataCase.ToLower().Trim();
        }

        public string GetTestDataOrValue(int index)
        {
            string data = GetData(index)[index];
            string dataCase = data.Contains("@") ? TestData.GetValue(data.TrimStart('@')) : data;
            return dataCase.Trim();
        }

        public bool GetTestDataOrDefaultValue(int index, bool @default)
        {
            var value = GetTestDataOrValueToLower(index);
            return string.IsNullOrWhiteSpace(value) ? @default : value.ToBool();
        }

        public int GetTestDataOrDefaultValue(int index, int @default)
        {
            var value = GetTestDataOrValueToLower(index);
            return string.IsNullOrWhiteSpace(value) ? @default : value.ToInt();
        }

        public int? GetTestDataOrNull(int index)
        {
            var value = GetTestDataOrValueToLower(index);
            int? returnvalue = value.ToInt();
            return string.IsNullOrWhiteSpace(value) ? null : returnvalue;
        }

        public DateTime GetTestDataDate(int index)
        {
            return GetTestData(index).ToDateTime();
        }

        public string GetTestData(string key, int? index = null)
        {
            if (TestStep.IsSharedStep)
                return TestData.GetValue(key);
            if (index.HasValue)
                return GetTestData(index.Value);
            throw new ArgumentException($"TestData does not contain {key} and no alternate index was given");
        }

        public int? GetTestDataNullableInt(string key, int? index = null)
        {
            var str = GetTestData(key, index);
            return str == "null" ? (int?) null : str.ToInt();
        }

        public IEnumerable<string> GetTestData(int index, char separator = ',')
        {
            var data = GetTestData(index);
            return string.IsNullOrWhiteSpace(data) ? new List<string>() : data.Trim().Split(separator).Select(x => x.Trim());
        }

        public List<string> GetMatchedItemsFromPattern(string testStepDescription)
        {
            var result = new List<string>();
            foreach(var pattern in (BehavePatternStep[]) Attribute.GetCustomAttributes(GetType(), typeof(BehavePatternStep)))
            {
                var match = pattern.GetMatchItems(testStepDescription);
                result.AddRange(match);
            }

            return result;
        }

        private bool HasBehavePatternStep()
        {
            return Attribute.GetCustomAttributes(GetType(), typeof(BehavePatternStep)).Any();
        }

        public void MapBehaviourPatternToTestData()
        {
            if (!HasBehavePatternStep())
                return;

            var regex = @"\[([^]]*)\]";
            var matchedItems = GetMatchedItemsFromPattern(TestStep.Description)[0];
            List<string> matches = Regex.Matches(matchedItems.Replace(Environment.NewLine, ""), regex).Cast<Match>().Select(x => x.Groups[1].Value).ToList();

            
            foreach(var match in matches)
            {
                var keyValue = match.Split(':');
                if (keyValue.Length != 2)
                    continue;

                TestData.Insert(keyValue[0], keyValue[1].Replace("\"", ""));
            }
        }

        protected byte[] GetAttachment(string name)
        {
            if (!TestData.Attachments.ContainsKey(name))
            {
                var hint = name.Contains("_attachment") ? "" : ", the key name must contain \"_attachment\"";
                throw new ArgumentException($"Could not find attachment '{name}'" + hint);
            }

            return TestData.Attachments[name];
        }

        protected string GetAttachmentAsString(string name)
        {
            var attachment = GetAttachment(name);
            var reader = new StreamReader(new MemoryStream(attachment), Encoding.Default);
            return reader.ReadToEnd();
        }

        protected XmlDocument GetAttachmentAsXml(string name)
        {
            var xml = new XmlDocument();
            var xmlData = GetAttachmentAsString(name);
            xml.LoadXml(xmlData);
            return xml;
        }

        protected Xlsx GetAttachmentAsXlsx(string name)
        {
            return new Xlsx(GetAttachment(name));
        }

        public string GetExpectedResult()
        {
            return TestStep.ExpectedResult?.TrimStart('@') ?? "";
        }

        public string[] GetExpectedResultList()
        {
            return GetExpectedResult().Split(',')
                .Where(res => !string.IsNullOrEmpty(res))
                .ToArray();
        }

        public int? GetExpectedResultNullableInt()
        {
            var expectedResultString = GetExpectedResult();
            return string.IsNullOrEmpty(expectedResultString) ? (int?)null : int.Parse(expectedResultString);
        }

        public virtual void Cleanup(TestStepResult result)
        {
        }

        public virtual TestStepResult Do()
        {
            if (_calledFromBase)
                throw new NotImplementedException();

            _calledFromBase = true;
            return DoAsync().GetAwaiter().GetResult();
        }

        public virtual Task<TestStepResult> DoAsync()
        {
            if (_calledFromBase)
                throw new NotImplementedException();

            return Task.FromResult(Do());
        }

        protected void TryExecuteDontCare(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch
            {
                // don't care
            }
        }


        protected TestStepResult Result(bool result, List<string> errorMessages, string errorPrefix = "")
        {
            return result ? TestStepResult.Successful() : TestStepResult.Failed($"{errorPrefix}\n{string.Join(Environment.NewLine, errorMessages)}");
        }

        protected void TryExecute(Action action, bool shouldFail = false, string expectedError = "", string description = null)
        {
            TryExecute<object>(() => { action.Invoke(); return null; }, shouldFail, expectedError, description);
        }

        protected TestStepResult TryExecute(Action action, string expectedError)
        {
            TryExecute(action, !string.IsNullOrWhiteSpace(expectedError), expectedError);
            return TestStepResult.Successful();
        }

        protected TestStepResult TryExecute<T>(Func<T> action, string expectedError, Action<T> postAction)
        {
            return TryExecute(action, !string.IsNullOrWhiteSpace(expectedError), expectedError, postAction);
        }


        protected TestStepResult TryExecute<T>(Func<T> action, bool shouldFail, string expectedError, Action<T> postAction)
        {
            var res = TryExecute(action, shouldFail, expectedError);
            if (!shouldFail) postAction.Invoke(res);

            return TestStepResult.Successful();
        }

        public async Task<TestStepResult> TryExecuteAsync<TException>(int numberOfTries, Func<Task> action, Func<bool> validResponseChecker = null, string customfailmsg = null) where TException : Exception
        {
            bool validRespons = false;
            Exception innerEx = null;
            for (int i = 0; i < numberOfTries && !validRespons; i++)
            {
                if(i != 0)
                    await Task.Delay(3000);

                try
                {
                    await action.Invoke();
                    validRespons = validResponseChecker == null || validResponseChecker.Invoke();
                }
                catch (TException e) when (i != numberOfTries)
                {
                    innerEx = e;
                    Console.WriteLine("TryExecuteAsync: exception caught ("+  e.Message + "). Trying again");
                }
            }

            if (!validRespons)
            {
                throw new Exception(customfailmsg ?? "Did not get valid response after " + numberOfTries + " tries", innerEx);
            }

            return TestStepResult.Successful();
        }

        protected async Task<TestStepResult> TryExecuteAsync<TReturnType, TException>(int numberOfTries, Func<Task<TReturnType>> action, Func<TReturnType, bool> validResponseChecker = null, string customfailmsg = null) where TException : Exception
        {
            bool validRespons = false;
            for (int i = 0; i < numberOfTries && !validRespons; i++)
            {
                if (i != 0)
                    await Task.Delay(3000);

                try
                {
                    var obj = await action.Invoke();
                    validRespons = validResponseChecker == null || validResponseChecker.Invoke(obj);
                }
                catch (TException e) when (i != numberOfTries)
                {
                    Console.WriteLine("TryExecuteAsync: exception caught (" + e.Message + "). Trying again");
                }
            }

            return validRespons ? TestStepResult.Successful() : TestStepResult.Failed(customfailmsg ?? "Did not get valid response after " + numberOfTries + " tries");
        }

        protected async Task<TestStepResult> TryExecuteAsync<T>(Func<Task<T>> action, bool shouldFail, string expectedError, Action<T> postAction)
        {
            var res = await TryExecuteAsync(async () => await action(), shouldFail, expectedError);
            if (!shouldFail) postAction.Invoke(res);

            return TestStepResult.Successful();
        }

        protected T TryExecute<T>(Func<T> action, bool shouldFail = false, string expectedError = "", string description = null)
        {
            T result;
            description = description ?? Description ?? $"utføre teststeg {GetType().Name}";
            try
            {
                result = action.Invoke();
            }
            catch (Exception e)
            {
                Assert.True(shouldFail, $"Kunne ikke {description} selv om det var forventet: {e.GetType()} {e.Message}");
                Assert.True(
                    e.Message.ToLower().Contains(expectedError.ToLower()),
                    $"Fikk tilbake feil feilmelding. Forventet '{expectedError}', fikk '{e.Message}'");
                return default(T);
            }
            Assert.True(!shouldFail, $"Kunne {description} selv om det ikke var forventet");
            return result;
        }

        protected async Task<T> TryExecuteAsync<T>(Func<Task<T>> action, bool shouldFail = false, string expectedError = "", string description = null)
        {
            T result;
            description = description ?? Description ?? $"utføre teststeg {GetType().Name}";
            try
            {
                result = await action.Invoke();
            }
            catch (Exception e)
            {
                Assert.True(shouldFail, $"Kunne ikke {description} selv om det var forventet: {e.GetType()} {e.Message}");
                Assert.True(
                    e.Message.ToLower().Contains(expectedError.ToLower()),
                    $"Fikk tilbake feil feilmelding. Forventet '{expectedError}', fikk '{e.Message}'");
                return default(T);
            }
            Assert.True(!shouldFail, $"Kunne {description} selv om det ikke var forventet");
            return result;
        }

        #region GUI/Webdriver


        protected string GetUrl()
        {
            return WebDriver.Url;
        }

        public void ClickElementByJavascript(By by)
        {
            ClickElementByJavascript(WebDriver.FindElement(by));
        }

        public void ClickElementByJavascript(IWebElement element)
        {
            // Denne metoden er en workaround for problem med "element is not clickable"
            // http://stackoverflow.com/questions/38923356/element-is-not-clickable-at-point-other-element-would-receive-the-click
            Assert.True(element != null, "Cannot click null element");
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].click()", element);
        }

        public void ClickElement(IWebElement element)
        {
            Assert.True(element != null, "Cannot click null element");
            new Actions(WebDriver).MoveToElement(element).Perform();
            element.WaitUntilVisible(TimeSpan.FromSeconds(5));
            element.Click();
        }

        public void ClickElement(By by, TimeSpan? wait = null)
        {
            WaitUntilVisible(by, wait ?? TimeSpan.FromSeconds(5));
            var element = WebDriver.FindElement(by);
            Assert.True(element != null, "Cannot click null element");
            new Actions(WebDriver).MoveToElement(element).Perform();
            element.Click();
        }

        //Hack for obscured elements (use with caution)
        public void ClickWithKeyboard(By by)
        {
            var element = WebDriver.FindElement(by);
            new Actions(WebDriver).MoveToElement(element).Perform();
            WaitUntilVisible(by, TimeSpan.FromSeconds(5));
            element.SendKeys(Keys.Return);
        }

        public void ClickElements(By by)
        {
            foreach (var element in WebDriver.FindElements(by))
            {
                element.Click();
            }
        }

        public void InputText(By by, string text, bool clearInput = true)
        {
            WebDriver
                .FindElement(by)
                .WaitUntilVisible(TimeSpan.FromSeconds(2))
                .InputText(text, clearInput);
        }

        public void InputText(IWebElement element, string text, bool clearInput = true)
        {
            Assert.True(element != null, "Element is null");
            if (clearInput)
            {
                element.Clear();
            }
            element.SendKeys(text);
        }

        public void AssertIsOnPage(bool expectedSuccess, params string[] pageText)
        {
            var msg = string.Format("Kom {0} inn på siden selv om det {1} var forventet, kunne {0} se teksten",
                expectedSuccess ? "ikke " : "",
                expectedSuccess ? "" : "ikke ");

            foreach (var text in pageText)
            {
                Assert.True(expectedSuccess == CanSeeText(text), $"{msg} '{text}'");
            }
        }

        public bool IsElementPresent(By by, TimeSpan? wait = null)
        {
            if (wait != null)
            {
                WebDriver.Manage().Timeouts().ImplicitlyWait(wait.Value);
            }
            WebDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
            return WebDriver.FindElements(by).Count > 0;
        }

        public void WaitUntil(Func<bool> condition, string description, TimeSpan? wait = null)
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

        public void WaitUntilCanSeeText(string text, int secondsToWait = 10)
        {
            WaitUntil(() => CanSeeText(text), $"Waiting to see text {text}", TimeSpan.FromSeconds(secondsToWait));
        }

        public IWebElement WaitUntilVisible(By by, TimeSpan? wait = null)
        {
            var webWait = new WebDriverWait(WebDriver, wait ?? TimeSpan.FromSeconds(10));
            return webWait.Until(ExpectedConditions.ElementIsVisible(by));
        }

        public void WaitUntilNotVisible(By by, TimeSpan wait)
        {
            WebDriver.Manage().Timeouts().ImplicitlyWait(wait + TimeSpan.FromMilliseconds(50));
            var webWait = new WebDriverWait(WebDriver, wait);
            webWait.Until(ExpectedConditions.InvisibilityOfElementLocated(by));
            WebDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
        }

        public string FindErrorMessage(string errorMessage)
        {
            var errorElements = WebDriver.FindElements(By.XPath($"//*[text()[contains(.,'{errorMessage}')]]"));
            return string.Join(", ", errorElements.Select(e => e.Text));
        }

        public bool CanSeeText(string text, TimeSpan? timeSpan = null)
        {
            var xpathSearchText = $"//*[text()[contains(.,'{text}')]]";
            return IsElementPresent(By.XPath(xpathSearchText), timeSpan);
        }

        public virtual void GoToUrl(string url, string baseUrl = null)
        {
            if (!url.StartsWith("http"))
            {
                if (baseUrl == null)
                    throw new Exception("Baseurl was null");

                url = baseUrl + url;
            }

            if (WebDriver.Url != url)
            {
                WebDriver.Navigate().GoToUrl(url);
            }
        }

        public void Search(string url, string searchQuery, int secondsToWait = 10)
        {
            GoToUrl(url);

            var searchInputField = By.Name("Q");
            var searchResult = By.Id("search-results");
            var notFoundElement = By.XPath("//*[@ng-show=\"vm.notFound\"]");
            var errorMessageXPath = By.XPath("//*[@ng-show=\"vm.errorMessage\"]");

            InputText(searchInputField, searchQuery + Keys.Return);
            WaitUntilVisible(searchResult, TimeSpan.FromSeconds(secondsToWait));

            Assert.True(IsVisible(searchResult) || IsVisible(notFoundElement), "Kunne ikke se søkeresultatet eller ikke funnet.");
            Assert.True(IsNotVisible(errorMessageXPath), "Kunne se feilmelding selv om det ikke var forventet.");
        }

        public void ExportToCSV(string url)
        {
            GoToUrl(url);
            var exportButton = By.XPath("//a[text()='Exporter til CSV']");
            WaitUntilVisible(exportButton, TimeSpan.FromSeconds(20));
            LinkHasStatusCode(exportButton, HttpStatusCode.OK);
        }

        public void LinkHasStatusCode(By by, HttpStatusCode expected)
        {
            var element = WebDriver.FindElement(by);
            var url = element.GetAttribute("href");
            try
            {
                using (var response = WebRequest.Create(url).GetResponse() as HttpWebResponse)
                {
                    Assert.Equal(expected, response.StatusCode, "Kall mot URL ga feil statuskode");
                }
            }
            catch (WebException e)
            {
                Assert.Fail($"Kall mot URL feilet: {e.Message}");
            }
            Assert.True(element.Enabled, "Linken i elementet virket, men elementet er ikke enabled");
        }

        public bool IsVisible(By by)
        {
            return WebDriver.FindElement(by).IsVisible();
        }

        public bool IsNotVisible(By by)
        {
            return !WebDriver.FindElement(by).IsVisible();
        }

        public void ScrollBy(int x, int y)
        {
            ((IJavaScriptExecutor)WebDriver).ExecuteScript($"scroll({x},{y});");
        }

        public void ScrollElementBy(By by, int x, int y)
        {
            var element = WebDriver.FindElement(by);
            ScrollElementBy(element, x, y);
        }

        public void ScrollElementBy(IWebElement element, int x, int y)
        {
            ExecuteJs($"arguments[0].scrollLeft += {x}", element);
            ExecuteJs($"arguments[0].scrollTop += {y}", element);
        }

      

        protected string[] GetOptionsInSelect(By by)
        {
            var returnList = new List<string>();

            foreach (var option in WebDriver.FindElement(by).FindElements(By.TagName("option")))
            {
                returnList.Add(option.Text);
            }

            return returnList.ToArray();
        }

        protected void ExpectAlert()
        {
            if (!(WebDriver is PhantomJSDriver)) return;

            const string injectAlertTextInDOM = @"   
                    window.alert = function(str) {
                        if(jQuery('#alertText').length === 0) {
                            jQuery('body').append(jQuery('<input>').attr('id', 'alertText').attr('text', 'hidden'));
                        }
                        jQuery('#alertText').text(str);                        
                    }";
            ExecuteJs(injectAlertTextInDOM);
        }

        protected object ExecuteJs(string script, params object[] args)
        {
            var js = WebDriver as IJavaScriptExecutor;
            return js.ExecuteScript(script, args);
        }

        protected string GetAlertText(int secondsToWait = 3, bool accept = true)
        {
            if (WebDriver is PhantomJSDriver)
            {
                return WebDriver.FindElement(By.Id("alertText")).Text;
            }
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(secondsToWait));
            wait.Until(ExpectedConditions.AlertIsPresent());
            var alert = WebDriver.SwitchTo().Alert();
            var alertText = alert.Text; //Have to set text to a variable before we close it
            if (accept)
            {
                alert.Accept();
            }
            return alertText;
        }

        protected void SelectOptionByText(By bySelect, string optionText)
        {
            ClickElement(bySelect);
            var select = WebDriver.FindElement(bySelect);
            ClickElement(select.FindElement(By.XPath($"option[text()='{optionText}']")));
        }

        protected void SetTimeout(TimeSpan timeout)
        {
            WebDriver.Manage().Timeouts().SetPageLoadTimeout(timeout);
        }

        #endregion
    }
}


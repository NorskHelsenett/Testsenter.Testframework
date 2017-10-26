using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Diagnostics;
using Shared.Common.Storage;
using TestFramework.Interfaces;
using TestFramework.Resources;

namespace TestFramework.Core
{
    public class TestcaseAttachmentCollector : IAttachmentCollector
    {
        private readonly IBlobStorageDb _blobStore;

        public TestcaseAttachmentCollector(IBlobStorageDb blobStore)
        {
            _blobStore = blobStore;
        }

        public string GetReferenceToAttachmentIfApplicable(TestState state)
        {
            if (IsGuiTest(state))
                return GetScreenShot(state);

            return null;
        }

        private string GetScreenShot(TestState state)
        {
            try
            {
                var driver = (IWebDriver)state.GetInstanceWithKey("driver");
                var bytes = GetScreenshot(driver).AsByteArray;

                return _blobStore.UploadFile(bytes, name: Guid.NewGuid().ToString() + ".png").Split('?')[0];
            }
            catch (Exception e)
            {
                Trace.WriteLine("Attachments: " + e.Message + ", stack: " + e.StackTrace);
                return null;
            }
        }

        public static Screenshot GetScreenshot(IWebDriver driver)
        {
            if (driver is RemoteWebDriver)
                return ((RemoteWebDriver)driver).GetScreenshot(); //phantomjs

            if (driver is ITakesScreenshot)
                return ((ITakesScreenshot)driver).GetScreenshot(); //others

            return null;
        }

        private bool IsGuiTest(TestState state)
        {
            return state.HasInstance<IWebDriver>();
        }

        public static void TrySaveLocalScreenshot(string path, IWebDriver driver)
        {
            try
            {
                SaveLocalScreenshot(path, driver);
            }
            catch (Exception) { }
        }

        public static void SaveLocalScreenshot(string path, IWebDriver driver)
        {
            var ss = GetScreenshot(driver);
            ss.SaveAsFile(path + @"\" + Guid.NewGuid().ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}


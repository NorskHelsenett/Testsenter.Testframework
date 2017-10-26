using Shared.Common.DI;
using Shared.Common.Resources;
using Shared.Common.Storage;
using TestFramework.Core;
using TestFramework.Interfaces;
using TestFramework.Selenium;
using log4net;
using Shared.Common.Testing;
using TestFramework.CommonSteps;
using TestFramework.Resources;

namespace TestFramework
{
    public class DiRegistrations : IExternalProjectRegistration, ITestRegistration
    {
        private readonly PreferredSeleniumBrowser _browser;

        public DiRegistrations() 
        {
            _browser = PreferredSeleniumBrowser.PhantomJS;
        }

        public DiRegistrations(PreferredSeleniumBrowser browser)
        {
            _browser = browser;
        }
        public void Register(UnityDependencyInjector di, ServiceDescription caller)
        {
            di.Register<IAttachmentCollector, TestcaseAttachmentCollector>(InstanceLifetime.ReturnSameInstanceForEachResolve, new DiResolveArg(typeof(IBlobStorageDb), "Attachments"));
            di.Register<ITestStepFactory, TestStepFactory>();
            di.Register<IWebDriverProvider, SimpleWebDriverProvider>(InstanceLifetime.ReturnNewInstanceForEachResolve, "SimpleWebDriverProvider", _browser);
            di.Register<IWebDriverProvider, WebDriverProvider>(InstanceLifetime.ReturnSameInstanceForEachResolve, "WebDriverProvider", 5, 20, _browser, di.GetInstance<ILog>());
            di.Register<ITestExecutor, TestExecutor>(InstanceLifetime.ReturnNewInstanceForEachResolve, di.GetInstance<ILog>(), di.GetInstance<ITestStepFactory>(), di.GetInstance<IAttachmentCollector>(), di.GetInstance<IWebDriverProvider>("SimpleWebDriverProvider"));
            di.Register<IAsyncTestExecutor, ParallellTestExecutor>(InstanceLifetime.ReturnNewInstanceForEachResolve, di.GetInstance<ILog>(), di.GetInstance<ITestStepFactory>(), di.GetInstance<IAttachmentCollector>(), di.GetInstance<ITestExecutorReporting>(), di.GetInstance<IWebDriverProvider>("WebDriverProvider"));
        }

        public void Dispose(UnityDependencyInjector di)
        {
            foreach (var webDriverProvider in di.GetAllInstancesOf<IWebDriverProvider>())
                webDriverProvider.Cleanup();
        }
        
        public void RegisterTests(ITestInjector injector)
        {
            injector.RegisterTest<StegMedTestDataComment>(131353);
        }
    }
}

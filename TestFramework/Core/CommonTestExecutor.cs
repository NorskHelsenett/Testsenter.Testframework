using Shared.Common.Resources;
using log4net;
using TestFramework.Interfaces;
using TestFramework.Resources;
using Shared.Common.Testing;

namespace TestFramework.Core
{
    public class CommonTestExecutor
    {
        protected readonly ITestStepFactory TestStepFactory;
        protected readonly IWebDriverProvider WebDriverProvider;
        protected readonly IAttachmentCollector ErrorCollector;
        protected ILog Log;

        public CommonTestExecutor(ILog log, ITestStepFactory testStepFactory, IAttachmentCollector errorCollector, IWebDriverProvider webDriverProvider)
        {
            WebDriverProvider = webDriverProvider;
            TestStepFactory = testStepFactory;
            ErrorCollector = errorCollector;
            Log = log;
        }

        protected void CleanupTestStep(Point point, TestStep step, TestState testState)
        {
            var executor = GetStepExecutor(point, step);
            executor.TestState = testState;
            executor.Cleanup(step.Result);
        }

        protected IExecuteTestStep GetStepExecutor(Point point, TestStep step)
        {
            step.FoundInEnvironment = point.Environment;

            var executor = TestStepFactory.GetTestStepExecutor(step);
            executor.TestStep = step;
            executor.TestData = point.Data;

            executor.Init();

            return executor;
        }
    }
}

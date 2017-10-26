using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Shared.Common.Resources;
using TestFramework.Interfaces;
using TestFramework.Resources;
using TestFramework.Resources.Attributes;
using OpenQA.Selenium;
using Shared.Common.Testing;

namespace TestFramework.Core
{
    public class TestExecutor : CommonTestExecutor, ITestExecutor
    {
        private TestRun _testRun;
        private int[] _testIds;

        public TestExecutor(ILog log, ITestStepFactory testStepFactory, IAttachmentCollector errorCollector, IWebDriverProvider webDriverProvider)
            : base(log, testStepFactory, errorCollector, webDriverProvider)
        { }

        public TestRun RunAllTests(TestRun testRun, bool suppressExceptions, int[] testIds)
        {
            _testRun = testRun;

            if (testIds != null && testIds.Length > 0)
            {
                _testIds = testIds;
                _testRun.Tests = _testRun.Tests.Where(t => testIds.Contains(t.TestCaseId)).ToList();
            }

            return RunTests(suppressExceptions, _testRun.Tests);
        }

        public TestRun RunAllTests(TestRun testRun, bool suppressExceptions)
        {
            _testRun = testRun;

            return RunTests(suppressExceptions, _testRun.Tests);
        }

        private TestRun RunTests(bool suppressExceptions, List<Test> tests)
        {
            _testRun.Started = DateTime.UtcNow;
            foreach (var test in tests)
            {
                Log.Info($"Kjører Test: {test.TestCaseId} - {test.TestCaseName}");
                Run(test, suppressExceptions);
            }

            _testRun.Finished = DateTime.UtcNow;
            return _testRun;
        }

        public void Run(Test test, bool suppressExceptions)
        {
            IWebDriver claimedWebdriver = null;
            string driverId = "";
            bool cleanupSucceeded = false;

            foreach (var point in test.Points)
            {
                var testState = new TestState();
                if (claimedWebdriver != null)
                {
                    testState.SaveInstanceWithKey("driver", claimedWebdriver);
                    testState.SaveInstanceWithKey("driverid", driverId);
                }

                cleanupSucceeded = Do(point, suppressExceptions, testState);

                claimedWebdriver = (IWebDriver)testState.GetInstanceWithKey("driver");
                driverId = (string)testState.GetInstanceWithKey("driverid");
            }

            if (claimedWebdriver != null)
                WebDriverProvider.Release(claimedWebdriver, cleanupSucceeded);
        }

        private bool Do(Point point, bool suppressExceptionsIntoResult, TestState testState)
        {
            foreach (var step in point.TestSteps)
            {
                point.RunOk = true;

                if (!suppressExceptionsIntoResult)
                {
                    testState = ExecuteTestStep(point, step, testState);
                }
                else
                {

                    try
                    {
                        testState = ExecuteTestStep(point, step, testState);
                    }

                    catch (Exception e)
                    {
                        step.Result = e is TestCaseException ? TestStepResult.Failed(e.Message) : TestStepResult.ImplementationError(e);
                        step.Result.RefToScreenshot = ErrorCollector.GetReferenceToAttachmentIfApplicable(testState);
                        step.Result.SetException(e);
                        point.RunOk = false;
                        break;
                    }
                }

                if(!step.Result.Success)
                {
                    step.Result.RefToScreenshot = ErrorCollector.GetReferenceToAttachmentIfApplicable(testState);
                    point.RunOk = false;
                    break;
                }
            }

            bool cleanupSucceeded = true;
            for (var i = point.TestSteps.Count -1; i >= 0; i--)
            {
                var step = point.TestSteps[i];
                try
                {
                    CleanupTestStep(point, step, testState);
                }
                catch (Exception e)
                {
                    Log.Info($"Cleanup Failed for Point {point.Id} in Step {step.StepIndex}: {e.Message}");
                    cleanupSucceeded = false;
                }
            }

            return cleanupSucceeded;
        }

        private TestState ExecuteTestStep(Point point, TestStep step, TestState testState)
        {
            var executor = GetStepExecutor(point, step);
            AddTestStateToExecutor(executor, testState);

            step.Result = executor.Do();

            if(!step.Result.Success && testState != null)
            {
                var stateAsJson = testState.GetStateObjectsAsJson();
                if (string.IsNullOrEmpty(step.Result.AddtionalInformation))
                    step.Result.AddtionalInformation = stateAsJson;
                else
                    step.Result.AddtionalInformation += stateAsJson;
            }

            point.Data = executor.TestData;
            testState = executor.TestState;

            return testState;
        }
        private void AddTestStateToExecutor(IExecuteTestStep executor, TestState testState)
        {
            if (testState.GetInstanceWithKey("driver") == null && Attribute.GetCustomAttribute(executor.GetType(), typeof(InitWebDriver)) != null)
            {

                var webDriver = WebDriverProvider.Get(testState.SessionId).Result;
                var id = Guid.NewGuid().ToString();

                testState.SaveInstanceWithKey("driver", webDriver);
                testState.SaveInstanceWithKey("driverid", id);
            }

            executor.TestState = testState;
        }
    }
}

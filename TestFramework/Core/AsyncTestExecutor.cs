using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TestFramework.Interfaces;
using TestFramework.Resources;
using TestFramework.Resources.Attributes;
using OpenQA.Selenium;
using log4net;
using Shared.Common.Testing;

namespace TestFramework.Core
{
    public class AsyncTestExecutor : CommonTestExecutor
    {
        private TestRun _testRun;
        private int[] _testIds;
        private readonly ILog _logger;


        public AsyncTestExecutor(ILog logger, ITestStepFactory testStepFactory, IAttachmentCollector errorCollector, IWebDriverProvider webDriverProvider)
            : base(logger, testStepFactory, errorCollector, webDriverProvider)
        {
            _logger = logger;
        }


        public async Task<TechnicalOutcome> Run(Test test, bool suppressExceptions, CancellationToken cancelToken)
        {
            IWebDriver claimedWebdriver = null;
            string driverId = "";
            TechnicalOutcome outcome = TechnicalOutcome.Ok;

            foreach (var point in test.Points)
            {
                var testState = new TestState();
                if (claimedWebdriver != null)
                {
                    testState.SaveInstanceWithKey("driver", claimedWebdriver);
                    testState.SaveInstanceWithKey("driverid", driverId);
                }

                var stopwatch = new Stopwatch();

                stopwatch.Start();
                outcome = await Do(point, suppressExceptions, testState, test.TestCaseId, cancelToken);
                stopwatch.Stop();
                point.Duration = stopwatch.Elapsed;

                claimedWebdriver = (IWebDriver)testState.GetInstanceWithKey("driver");
                driverId = (string) testState.GetInstanceWithKey("driverid");

                if (outcome != TechnicalOutcome.Ok)
                    break;
            }

            if (claimedWebdriver != null)
            {
                _logger.Debug($"Releasing driver {driverId}");
                WebDriverProvider.Release(claimedWebdriver, outcome == TechnicalOutcome.Ok);
                _logger.Debug($"Driver {driverId} released");
            }

            return outcome;
        }

        private void LogRunAgainException(Exception we, int testCaseId)
        {
              _logger.Warn("While running test " + testCaseId + ": got exception " + we.Message + ". This exception is tagged as RunAgain. Rerunning", we);
        }

        private async Task<TechnicalOutcome> Do(Point point, bool suppressExceptionsIntoResult, TestState testState, int testCaseId, CancellationToken cancelToken)
        {
            foreach (var step in point.TestSteps)
            {
                if (cancelToken.IsCancellationRequested) break;
                point.RunOk = true;

                if (!suppressExceptionsIntoResult)
                {
                    testState = await ExecuteTestStep(point, step, testState, cancelToken);
                }
                else
                {

                    try
                    {
                        testState = await ExecuteTestStep(point, step, testState, cancelToken);
                    }

                    catch (WebDriverException we) when (we.Message.Contains("Variable Resource Not Found"))
                    {
                        LogRunAgainException(we, testCaseId);
                        return TechnicalOutcome.RunAgain;
                    }

                    catch (WebDriverException we) when (we.Message.Contains("Only one usage of each socket address"))
                    {
                        LogRunAgainException(we, testCaseId);
                        return TechnicalOutcome.RunAgain;
                    }

                    catch (WebDriverException we)
                        when (
                            we.Message.Contains("The HTTP request to the remote WebDriver server for URL") &&
                            we.Message.Contains("timed out"))
                    {
                        LogRunAgainException(we, testCaseId);
                        return TechnicalOutcome.RunAgain;
                    }

                    catch (NoSuchWindowException we)
                    {
                        LogRunAgainException(we, testCaseId);
                        return TechnicalOutcome.RunAgain;
                    }

                    catch(FailureToStartTestException we)
                    {
                        LogRunAgainException(we, testCaseId);
                        return TechnicalOutcome.RunAgain;
                    }

                    catch (InvalidOperationException we) when (we.Message.Contains("unable to send message to renderer"))
                    {
                        LogRunAgainException(we, testCaseId);
                        return TechnicalOutcome.RunAgain;
                    }

                    catch (Exception e)
                    {
                        step.Result = e is TestCaseException ? TestStepResult.Failed(e.Message) : TestStepResult.ImplementationError(e);
                        step.Result.RefToScreenshot = ErrorCollector.GetReferenceToAttachmentIfApplicable(testState);
                        Console.WriteLine("For point in testcase " + point.Id + "; screenshot uploaded to " + step.Result.RefToScreenshot);
                        step.Result.SetException(e);
                        point.RunOk = false;
                        _logger.Error(e);

                        break;
                    }
                }


                if (!step.Result.Success)
                {
                    step.Result.RefToScreenshot = ErrorCollector.GetReferenceToAttachmentIfApplicable(testState);
                    Console.WriteLine("For point in testcase " + point.Id + "; screenshot uploaded to " + step.Result.RefToScreenshot);
                    point.RunOk = false;
                    break;
                }
            }

            for (var i = point.TestSteps.Count - 1; i >= 0; i--)
            {
                var step = point.TestSteps[i];
                try
                {
                    CleanupTestStep(point, step, testState);
                }
                catch (Exception e)
                {
                    _logger.Error($"Cleanup Failed for Point {point.Id} in Step {step.StepIndex} of TC {testCaseId}: {e.Message}");
                }
            }

            return TechnicalOutcome.Ok; 
        }

        private async Task<TestState> ExecuteTestStep(Point point, TestStep step, TestState testState, CancellationToken cancelToken)
        {
            var executor = GetStepExecutor(point, step);
            await AddTestStateToExecutor(executor, testState);

            step.Result = await executor.DoAsync();

            if (!step.Result.Success && testState != null)
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

        private async Task<bool> AddTestStateToExecutor(IExecuteTestStep executor, TestState testState)
        {
            if (testState.GetInstanceWithKey("driver") == null && Attribute.GetCustomAttribute(executor.GetType(), typeof(InitWebDriver)) != null)
            {
                _logger.Debug("Claiming webdriver");

                var webDriver = await WebDriverProvider.Get(testState.SessionId);
                var id = Guid.NewGuid().ToString();

                _logger.Debug($"Webdriver with id {id} claimed");
                testState.SaveInstanceWithKey("driver", webDriver);
                testState.SaveInstanceWithKey("driverid", id);
            }

            executor.TestState = testState;

            return true;
        }
    }
}

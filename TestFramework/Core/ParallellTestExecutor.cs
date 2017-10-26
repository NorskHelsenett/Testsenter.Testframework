using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Common.Resources;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using TestFramework.Interfaces;
using Shared.Common.Log;
using log4net;
using Shared.Common.Testing;
using TestFramework.Resources;

namespace TestFramework.Core
{
    public class ParallellTestExecutor : IAsyncTestExecutor
    {
        private bool _suppressExceptions;
        private int _numberOfThreads;
        private int _targetCount;
        private int _passed;
        private int _failed;
        private readonly ITestStepFactory _testStepFactory;
        private readonly ITestExecutorReporting _reporting;
        private readonly IAttachmentCollector _errorCollector;
        private readonly IWebDriverProvider _webDriverProvider;
        private TestExecutionSchedule _testSchedule;
        private ConcurrentQueue<Test> _testDoneQueue;
        private bool _stop;
        private TestRun _finished;
        private ILog _logger;

        public static int MaxNumberOfTriesForTestCase = 5;
        private readonly TimeSpan _testRunTimeout = new TimeSpan(5, 0, 0);

        public ParallellTestExecutor(ILog log, ITestStepFactory testStepFactory, IAttachmentCollector errorCollector, ITestExecutorReporting reporting, IWebDriverProvider webDriverProvider)
        {
            _stop = false;
            _errorCollector = errorCollector;
            _testStepFactory = testStepFactory;
            _reporting = reporting;
            _logger = log;
            _webDriverProvider = webDriverProvider;
        }

        public async Task<TestRun> RunAllTests(TestConfiguration configuration, TestRun testRun, ILog logger)
        {
            _logger = logger;
            _numberOfThreads = configuration.NumberOfThreads == 0 ? 1 : configuration.NumberOfThreads;
            _suppressExceptions = configuration.SuppressExceptions;
            _reporting.SetInitiator(configuration.GetInitiator());

            if (configuration.TestIds != null && configuration.TestIds.Length > 0)
            {
                testRun.Tests = testRun.Tests.Where(t => configuration.TestIds.Contains(t.TestCaseId)).ToList();
            }

            _targetCount = testRun.Tests.Count;
            _passed = 0;
            _failed = 0;

            _logger.Debug($"{nameof(ParallellTestExecutor)} about to run test plan {testRun.TestPlanId}");
            return await RunTests(testRun, configuration.GetInitiator());
        }

        private async Task<TestRun> RunTests(TestRun testRun, Initiator initiator)
        {
            Task<bool> doneWorker = null;
            List<Task<bool>> testWorkers = null;

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            try
            {
                var header = GetHeader(testRun, initiator);
                var isStarted = await _reporting.Start(testRun, _numberOfThreads, header);

                _logger.Debug($"TestRun of plan {testRun.TestPlanId} is about to run with {_numberOfThreads} threads with the following exclusion groups: {string.Join(", ", testRun.Tests.Select(t => t.ExclusionGroup).Where(g => g != null).Distinct())}");

                _testDoneQueue = new ConcurrentQueue<Test>();
                _testSchedule = new TestExecutionSchedule(testRun.Tests);

                doneWorker = StartDoneWorker(testRun, token, header);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                testWorkers = StartTestWorkers(token, testRun.Tests.Count);

                if (await Timeout(testWorkers, _testRunTimeout, token))
                {
                    _logger.Error($"TestRun of plan {testRun.TestPlanId} timed out after {_testRunTimeout}");
                    CancelTestRun(tokenSource, testWorkers, doneWorker);
                }
                else
                {
                    stopwatch.Stop();
                    _logger.Info($"TestRun of plan {testRun.TestPlanId} finished succesfully after {stopwatch.Elapsed}");
                }

                _stop = true;
                await Task.WhenAll(doneWorker);

                await _reporting.Finished(_finished, _passed, _failed);
                

                return _finished;
            }
            catch (Exception e)
            {
                try
                {
                    CancelTestRun(tokenSource, testWorkers, doneWorker);
                }
                finally
                {
                    _logger.Error($"Failed running test plan {testRun.TestPlanId}: {e}");
                    await _reporting.Stop(_finished, _passed, _failed, _finished.Tests.Count, testRun.Tests.Count);
                }

                throw;
            }
        }

        private async void CancelTestRun(CancellationTokenSource tokenSource, List<Task<bool>> testWorkers, Task<bool> doneWorker)
        {
            tokenSource.Cancel();
            var timeout = new TimeSpan(0, 30, 0);
            if (testWorkers != null)
            {
                if (await Timeout(testWorkers, timeout, CancellationToken.None))
                {
                    _logger.Error($"Timed out after {timeout} while cancelling testWorkers");
                }
            }

            if (doneWorker != null)
            {
                if (await Timeout(new List<Task<bool>> {doneWorker}, timeout, CancellationToken.None))
                {
                    _logger.Error($"Timed out after {timeout} while cancelling doneWorker");
                }
            }
        }

        private async Task<bool> Timeout<T>(List<Task<T>> tasks, TimeSpan timeSpan, CancellationToken token)
        {
            var timeout = Task.Delay(timeSpan, token);
            return await Task.WhenAny(Task.WhenAll(tasks), timeout) == timeout;
        }

        private string GetHeader(TestRun testRun, Initiator initiator)
        {
            return testRun.Tests.Count != 1 ? 
                $"{initiator.GetCallerName()} kjører testplan {testRun.TestPlanId}" : 
                $"{initiator.GetCallerName()} kjører testcase {testRun.Tests.First().TestCaseName} ({testRun.Tests.First().TestCaseId})";
        }
        
        private List<Task<bool>> StartTestWorkers(CancellationToken cancelToken, int numberOfTests)
        {
            var numberOfThreads = Math.Min(_numberOfThreads, numberOfTests);
            _logger.Debug($"Starting {numberOfThreads} testworkers");

            var testWorkers = new List<Task<bool>>();

            for (int i = 0; i < numberOfThreads; i++)
            {
                var x = Task.Run(async () =>
                {
                    var executor = new AsyncTestExecutor(_logger, _testStepFactory, _errorCollector, _webDriverProvider);
                    Test test;

                    while (_testSchedule.Any())
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        if (_testSchedule.TryBeginTestExecution(out test))
                        {
                            test.NumberOfTries++;
                            _logger.Info($"Running test {test.TestCaseId} - {test.TestCaseName}");

                            var outcome = await executor.Run(test, _suppressExceptions, cancelToken);
                            _testSchedule.EndTestExcecution(test);

                            if (outcome == Resources.TechnicalOutcome.RunAgain && test.NumberOfTries < MaxNumberOfTriesForTestCase)
                            {
                                _logger.Error("Outcome for test with id " + test.TestCaseId + " was " + outcome.ToString() + ". Rerunning");
                                CleanTest(test);
                                _testSchedule.Enqueue(test);
                            }
                            else
                            {
                                foreach (var notRunPoint in test.Points.Where(point => point.TestSteps.Any(step => step == null)))
                                {
                                    notRunPoint.RunOk = false;
                                }

                                _testDoneQueue.Enqueue(test);
                                PrintDetailedTestResults(test);
                            }
                        }
                        else
                        {
                            await Task.Delay(500, cancelToken);
                        }
                    }

                    return true;
                }, cancelToken);

                testWorkers.Add(x);
            }

            return testWorkers;
        }

        private void CleanTest(Test test)
        {
            test.Points.ForEach(t => CleanPoint(t));
        }

        private void CleanPoint(Point p)
        {
            p.RunOk = false;
            p.TestSteps.ForEach(step => step.Result = null);
        }

        private static string GetDetailedTestResults(Test test)
        {
            var str = $"Results for test {test.TestCaseId} - {test.TestCaseName} (Status: {test.Status})\n";
            var l = test.Points.Count.ToString().Length;
            for (var i = 0; i < test.Points.Count; i++)
            {
                var point = test.Points[i];
                var pointId = (i + 1).ToString().PadRight(l, ' ');
                str += $"  Point {pointId}\n";
                var ll = point.TestSteps.Count.ToString().Length;
                for (var j = 0; j < point.TestSteps.Count; j++)
                {
                    var step = point.TestSteps[j];
                    var stepId = (j + 1).ToString().PadRight(ll, ' ');
                    if (step.Result?.Success ?? false)
                    {
                        str += $"    Step {stepId} Result: Passed\n";
                    }
                    else
                    {
                        var errorMessage = step.Result?.ErrorMessage ?? "No Error Message";
                        var failString = step.Result == null ? "No Result" : "Failure";
                        str += $"    Step {stepId} Result: {failString}\n      Error: {errorMessage}\n";
                    }
                }
            }
            return str;
        }

        private void PrintDetailedTestResults(Test test)
        {
            _logger.Info("\n" + GetDetailedTestResults(test));
        }

        private Task<bool> StartDoneWorker(TestRun testrun, CancellationToken cancelToken, string header)
        {
            var doneWorker = Task.Run(async () =>
            {
                _finished = new TestRun(testrun.TeamProject, testrun.TestPlanId, testrun.TfsUri, testrun.Id)
                {
                    Tests = new List<Test>(),
                    Id = testrun.Id,
                    Started = DateTime.UtcNow,
                    Header = header
                };

                Test test;
                while (!(_stop && _testDoneQueue.Count == 0))
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (_testDoneQueue.TryDequeue(out test))
                    {
                        await Report(test);
                    }
                    else
                    {
                        await Task.Delay(500, cancelToken);
                    }
                }

                _finished.Finished = DateTime.UtcNow;

                return true;
            }, cancelToken);


            return doneWorker;
        }

        private async Task<bool> Report(Test test)
        {
            if (test.Points.All(t => t.RunOk))
            {
                _passed++;
            }
            else
            {
                _failed++;
            }

            await _reporting.NextFinished(test, _finished, (_passed + _failed), _targetCount, _passed, _failed);
            _finished.Tests.Add(test);

            return true;
        }

        private TestRun GetFinishedTestRun()
        {
            return _finished;
        }
    }
}

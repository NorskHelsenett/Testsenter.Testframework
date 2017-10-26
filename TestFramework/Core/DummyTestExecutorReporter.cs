using System.Threading.Tasks;
using Shared.Common.Resources;
using Shared.Common.Testing;
using TestFramework.Resources;

namespace TestFramework.Core
{
    public class DummyTestExecutorReporter : ITestExecutorReporting
    {
        public Task<bool> AnyJobActiveForTestplan(int testPlanId)
        {
            return Task.FromResult(false);
        }

        public Task<bool> Finished(TestRun testRun, int passed, int failed)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Start(TestRun testRun, int numberOfThreads, string header)
        {
            return Task.FromResult(true);
        }

        public Task<bool> NextFinished(Test test, TestRun testRun, int progressCount, int targetCount, int passed, int failed)
        {
            return Task.FromResult(true);
        }

        public Task<string> Register(int testPlanId, Initiator initiator, string testRunId, string header)
        {
            return Task.FromResult("");
        }

        public void SetInitiator(Initiator initiator)
        {
        }

        public Task<bool> Stop(TestRun testRun, int passed, int failed, int progressCount, int targetCount)
        {
            return Task.FromResult(true);
        }
    }
}

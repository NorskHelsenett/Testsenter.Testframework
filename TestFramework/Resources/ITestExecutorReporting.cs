using System.Threading.Tasks;
using Shared.Common.Resources;
using Shared.Common.Testing;

namespace TestFramework.Resources
{
    public interface ITestExecutorReporting
    {
        Task<bool> AnyJobActiveForTestplan(int testPlanId);
        void SetInitiator(Initiator initiator);
        Task<string> Register(int testPlanId, Initiator initiator, string testRunId, string header);
        Task<bool> Start(TestRun testRun, int numberOfThreads, string header);
        Task<bool> NextFinished(Test test, TestRun testRun, int progressCount, int targetCount, int passed, int failed);
        Task<bool> Stop(TestRun testRun, int passed, int failed, int progressCount, int targetCount);
        Task<bool> Finished(TestRun testRun, int passed, int failed);
    }
}

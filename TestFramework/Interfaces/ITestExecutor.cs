using Shared.Common.Testing;
using TestFramework.Resources;

namespace TestFramework.Interfaces
{
    public interface ITestExecutor
    {
        TestRun RunAllTests(TestRun testRun, bool suppressExceptions);
        TestRun RunAllTests(TestRun testRun, bool suppressExceptions, int[] testIds);
        void Run(Test test, bool suppressExceptions);
    }
}

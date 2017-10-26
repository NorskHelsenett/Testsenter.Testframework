using log4net;
using Shared.Common.Log;
using Shared.Common.Resources;
using Shared.Common.Testing;
using System.Threading.Tasks;
using TestFramework.Resources;

namespace TestFramework.Interfaces
{
    public interface IAsyncTestExecutor
    {
        Task<TestRun> RunAllTests(TestConfiguration configuration, TestRun testRun, ILog logger);
    }
}

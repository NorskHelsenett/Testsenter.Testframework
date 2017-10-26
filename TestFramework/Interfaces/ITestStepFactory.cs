using Shared.Common.Resources;
using Shared.Common.Testing;
using TestFramework.Resources;

namespace TestFramework.Interfaces
{
    public interface ITestStepFactory
    {
        IExecuteTestStep GetTestStepExecutor(TestStep s);
    }
}

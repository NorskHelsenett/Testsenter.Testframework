using Shared.Common.Logic;
using Shared.Common.Resources;
using Shared.Common.Testing;
using System.Threading.Tasks;
using TestFramework.Resources;

namespace TestFramework.Interfaces
{
    public interface IExecuteTestStep
    {
        TestState TestState { get; set; }
        TestData TestData { get; set; }
        TestStep TestStep { get; set; }

        TestStepResult Do();
        Task<TestStepResult> DoAsync();
        void Cleanup(TestStepResult result);

        void Init();
    }
}

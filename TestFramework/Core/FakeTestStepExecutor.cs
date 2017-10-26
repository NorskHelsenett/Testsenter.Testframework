using System.Threading.Tasks;
using Shared.Common.Resources;
using TestFramework.Interfaces;
using TestFramework.Resources;
using Shared.Common.Testing;

namespace TestFramework.Core
{
    public class FakeTestStepExecutor : IExecuteTestStep
    {
        public TestState TestState
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        public TestData TestData
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        public TestStep TestStep
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        public void Cleanup(TestStepResult result)
        {
        }

        public void Init() { }
        
        public Task<TestStepResult> DoAsync()
        {
            return Task.FromResult(Do());
        }

        public TestStepResult Do()
        {
            return TestStepResult.Successful();
        }
    }
}

using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Behave;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Insert into teststate element with key (.*) and value (.*)")]
    public class InsertIntoTeststate : BaseExecuteTestStep
    {
        public InsertIntoTeststate(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            var key = GetTestData(1);
            var value = GetTestData(2);

            TestState.SaveInstanceWithKey(key, value);

            return TestStepResult.Successful();
        }
    }
}

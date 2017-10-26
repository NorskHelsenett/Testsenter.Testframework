using TestFramework.Behave;
using System.Threading;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Wait (.*) seconds")]
    class Wait : BaseExecuteTestStep
    {
        public Wait(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            Log.Info($"Seconds: {GetTestData(1)}");
            var seconds = int.Parse(GetTestData(1));
            Thread.Sleep(seconds * 1000);

            return TestStepResult.Successful();
        }
    }
}

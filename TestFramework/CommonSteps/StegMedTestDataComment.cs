using Shared.Common.DI;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    
    public class StegMedTestDataComment : BaseExecuteTestStep
    {
        private string _testdata_comment => TestData.GetValue("testdata_comment"); 


        public StegMedTestDataComment(IDependencyInjector di) : base(di)
        {
        }

        public override TestStepResult Do()
        {
            return TestStepResult.Successful();
        }
    }
}

using TestFramework.Selenium;
using TestFramework.Behave;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Check that element (.*) with selector (.*) exists in view")]
    class CheckElementExists : BaseExecuteTestStep
    {

        public string expectedResultPass = "Pass";
        public string expectedResultFail = "Fail";

        public CheckElementExists(IDependencyInjector injector) : base(injector)
        {

        }

        public override TestStepResult Do()
        {
            var elementQuery = GetTestData(1);
            var selectorType = GetTestData(2);

            var elementExists = IsElementPresent(SeleniumUtil.GetSelector(selectorType, elementQuery));

            var expectedResult = GetExpectedResult();
            Log.Info($"Expected result: '{expectedResult}'");

            //Expects that you want it to Pass if no Expected result is submitted
            if (expectedResult == "") expectedResult = "Pass";
            if (expectedResult == expectedResultPass)
            {
                if (elementExists)
                {
                    return TestStepResult.Failed($"Could not find Element {elementQuery}");
                }
            }
            else
            {
                if (!elementExists)
                {
                    return TestStepResult.Failed($"Could find Element {elementQuery} but it was not expected");
                }
            }

            return TestStepResult.Successful();
        }
    }
}

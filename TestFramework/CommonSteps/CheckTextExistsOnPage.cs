using TestFramework.Behave;
using OpenQA.Selenium;
using Shared.Common.DI;
using Shared.Common.Testing;
using TestFramework.Core;
using TestFramework.Resources;
using TestFramework.TestHelpers;

namespace TestFramework.CommonSteps
{
    [BehavePatternStep("Check that the text (.*) exists somewhere on the page")]
    public class CheckTextExistsOnPage : BaseExecuteTestStep
    {
        private string TextToFind => GetTestData(1);

        public CheckTextExistsOnPage(IDependencyInjector injector) : base(injector)
        {
        }

        public override TestStepResult Do()
        {
            TryExecute(
                description: $"see text {TextToFind} on page",
                shouldFail: !GetExpectedResult().ExpectsPass(@default: true),
                action: () => WaitUntilCanSeeText(TextToFind) 
                );

            return TestStepResult.Successful();
        }
    }
}
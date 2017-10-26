using System;
using System.Linq;

namespace TestFramework.Resources
{
    public class TestStep
    {
        public string Description { get; set; }
        public bool IsSharedStep { get; set; }
        public int ParentId { get; set; }
        public string ParentName { get; set; }
        public int StepId { get; set; }
        public TestStepResult Result { get; set; }
        public int StepIndex { get; set; }
        public string ExpectedResult { get; set; }
        public string FoundInEnvironment { get; set; }

        public static TestStep CreateStep(int testCaseId, int stepId, string description, int stepIndex, string expectedResult, string environment)
        {
            return new TestStep
            {
                IsSharedStep = false,
                ParentId = testCaseId,
                StepId = stepId,
                Description = description,
                StepIndex = stepIndex,
                ExpectedResult = expectedResult,
                FoundInEnvironment = environment
            };
        }

        public static TestStep CreateSharedStep(int sharedStepId, int stepId, string description, string environment, string parentName)
        {
            return new TestStep
            {
                IsSharedStep = true,
                ParentId = sharedStepId,
                StepId = stepId,
                Description = description,
                FoundInEnvironment = environment,
                ParentName = parentName
            };
        }

        public string FindFirstWordInDescriptionThatEndsWith(string suffix)
        {
            var punctuation = Description.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = Description.Split().Select(x => x.Trim(punctuation));

            return words.FirstOrDefault(w => w.EndsWith(suffix));
        }

        public string[] FindAllWordsInDescriptionThatEndsWith(string suffix)
        {
            var punctuation = Description.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = Description.Split().Select(x => x.Trim(punctuation));

            return words.Where(w => w.EndsWith(suffix)).ToArray();
        }
    }
}

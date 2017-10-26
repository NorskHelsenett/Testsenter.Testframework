using Shared.Common.Testing;

namespace TestFramework.Resources
{
    public class TestSummary
    {
        public int failedPoints { get; set; }
        public int failedTestCases { get; set; }
        public int failedSteps { get; set; }

        public int passedSteps { get; set; }
        public int passedPoints { get; set; }
        public int passedTestCases { get; set; }


        public TestSummary()
        {
            failedPoints = 0;
            failedTestCases = 0;
            failedSteps = 0;


            passedSteps = 0;
            passedPoints = 0;
            passedTestCases = 0;
        }

        public static TestSummary FromTestrun(TestRun testrun)
        {
            var returnSummary = new TestSummary();

            foreach (var test in testrun.Tests)
            {
                var failedTestCase = false;
                foreach (var point in test.Points)
                {
                    var failedPoint = false;
                    foreach (var step in point.TestSteps)
                    {
                        var failedStep = !step.Result?.Success ?? false;
                        failedPoint = failedStep || failedPoint;
                        failedTestCase = failedStep || failedTestCase;

                        if (failedStep)
                        {
                            returnSummary.failedSteps++;
                        }
                        else
                        {
                            returnSummary.passedSteps++;
                        }
                    }

                    if (failedPoint)
                    {
                        returnSummary.failedPoints++;
                    }
                    else
                    {
                        returnSummary.passedPoints++;
                    }

                }
                if (failedTestCase)
                {
                    returnSummary.failedTestCases++;
                }
                else
                {
                    returnSummary.passedTestCases++;
                }
            }

            return returnSummary;
        }

        public override string ToString()
        {
            var returnstring = "";

            var result = failedSteps == 0 ? "Success" : "Failure";
            returnstring += "|====================================|\n";
            returnstring += "|            Test Summary            |\n";
            returnstring += "|====================================|\n";
            returnstring += $"|Total Steps:{passedSteps + failedSteps,24}|\n";
            returnstring += $"|Passed Steps:{passedSteps,23}|\n";
            returnstring += $"|Failed Steps:{failedSteps,23}|\n";
            returnstring += "|====================================|\n";
            returnstring += $"|Total Scenarios:{passedPoints + failedPoints,20}|\n";
            returnstring += $"|Passed Scenarios:{passedPoints,19}|\n";
            returnstring += $"|Failed Scenarios:{failedPoints,19}|\n";
            returnstring += "|====================================|\n";
            returnstring += $"|Total TestCases:{passedTestCases + failedTestCases,20}|\n";
            returnstring += $"|Passed TestCases:{passedTestCases,19}|\n";
            returnstring += $"|Failed TestCases:{failedTestCases,19}|\n";
            returnstring += "|====================================|\n";
            returnstring += $"|{result,-36}|\n";
            returnstring += "|====================================|\n";

            return returnstring;
        }
    }
}

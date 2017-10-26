using System.Linq;
using Shared.Common.Testing;

namespace TestFramework.Resources
{
    public class ShortTest 
    {
        public string TestCaseName { get; set; }
        public int TestCaseId { get; set; }
        public string Environment { get; set; }
        public int NumberOfSteps { get; set; }
        public string Status { get; set; }
        public bool RunOk { get; set; }
        public int TestPlanId { get; set; }

        public ShortTest() { }

        public ShortTest(Test test, TestRun testRun)
        {
            TestCaseName = test.TestCaseName;
            TestCaseId = test.TestCaseId;
            Status = test.Status;
            TestPlanId = testRun.TestPlanId;

            if(test.Points.Any())
            {
                NumberOfSteps = test.Points.First().TestSteps.Count;
                Environment = test.Points.First().Environment;
                RunOk = test.Points.Any(p => !p.RunOk) ? test.Points.First(p => !p.RunOk).RunOk : test.Points.Last().RunOk;
            }
        }
    }
}

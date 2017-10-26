using System;
using Shared.Common.Storage;

namespace TestFramework.Resources
{
    public class TestStepResultBlob : IJsonStorageEntity
    {
        public int TestRunId { get; set; }
        public int TestCaseId { get; set; }
        public int IterationCount { get; set; }
        public int StepCount { get; set; }
        public string RefToScreenshot { get; set; }
        public string AddtionalInformation { get; set; }
        public string Exception { get; set; }

        public TestStepResultBlob() { }

        public TestStepResultBlob(int stepCount, string refToScreenshot, string additionalInformation, Exception e)
        {
            StepCount = stepCount;
            RefToScreenshot = refToScreenshot;
            AddtionalInformation = additionalInformation;
            if (e != null)
            {
                string msg = $"Exception: {e.Message}, Stack: {e.StackTrace}";
                if (e.InnerException != null)
                    msg += $". Inner: {e.InnerException.Message}, Stack: {e.InnerException.StackTrace}";

                Exception = msg;
            }
        }

        public void SetKeys(int testRunId, int testCaseId, int iterationCount)
        {
            TestRunId = testRunId;
            TestCaseId = testCaseId;
            IterationCount = iterationCount;
        }

        public string GetPartitionKey()
        {
            return ConstructPartitionKey(this);
        }

        public string GetRowKey()
        {
            return IterationCount.ToString() + "_" + StepCount.ToString();
        }

        public static string ConstructPartitionKey(TestStepResultBlob obj)
        {
            return ConstructPartitionKey(obj.TestRunId, obj.TestCaseId);
        }

        public static string ConstructPartitionKey(int testRunId, int testCaseId)
        {
            return testRunId.ToString() + "_" + testCaseId.ToString();
        }
    }
}

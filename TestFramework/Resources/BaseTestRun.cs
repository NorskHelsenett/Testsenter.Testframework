using System;
using Newtonsoft.Json;
using Shared.Common.Resources;
using Shared.Common.Storage;

namespace TestFramework.Resources
{
    public class BaseTestRun : IJsonStorageEntity
    {
        public int NumberOfTests { get; set; }
        public string Id { get; set; }
        public string TeamProject { get; set; }
        public string Project { get; set; }
        public int TestPlanId { get; set; }
        public string TfsUri { get; set; }
        public int ProgressStatusInt { get; set; }
        public bool ForDeployment { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public string Header { get; set; }
        public string BlobRef { get; set; }
        public string State { get; set; }

        [JsonIgnore]
        public Status ProgressStatus
        {
            get
            {
                return (Status)ProgressStatusInt;
            }
            set
            {
                ProgressStatusInt = (int)value;
            }
        }

        public BaseTestRun() { }

        public BaseTestRun(string teamProject, int testplanId, string tfsuri, string testrunId)
        {
            TeamProject = teamProject;
            TestPlanId = testplanId;
            TfsUri = tfsuri;
            Id = testrunId;
        }

        public string GetPartitionKey()
        {
            return TestPlanId.ToString();
        }

        public string GetRowKey()
        {
            return Id;
        }

        public static BaseTestRun AsBaseTestRun(TestRun a)
        {
            var b = new BaseTestRun(a.TeamProject, a.TestPlanId, a.TfsUri, a.Id)
            {
                ProgressStatus = a.ProgressStatus,
                NumberOfTests = a.Tests.Count
            };

            return b;
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Common.Storage;
using Shared.Common.Testing;

namespace TestFramework.Resources
{
    public class TestStepResultBlobStore : ITestStepResultBlobStore
    {
        private readonly IJsonStorage<TestStepResultBlob> _testStepResultStore;

        public TestStepResultBlobStore(IJsonStorage<TestStepResultBlob> testStepResultBlobstore)
        {
            _testStepResultStore = testStepResultBlobstore;
        }

        public void SaveErrorInformationBlob(int testRunId, int testCaseId, int iteration, TestStepResultBlob obj)
        {
            obj.SetKeys(testRunId, testCaseId, iteration);
            _testStepResultStore.Post(obj);
        }

        public async Task<IEnumerable<TestStepResultBlob>> GetErrorInformationBlobs(int testRunId, int testCaseId)
        {
            return await _testStepResultStore.Get(TestStepResultBlob.ConstructPartitionKey(testRunId, testCaseId));
        }
    }
}

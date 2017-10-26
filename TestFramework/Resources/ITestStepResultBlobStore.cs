using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestFramework.Resources
{
    public interface ITestStepResultBlobStore
    {
        void SaveErrorInformationBlob(int testRunId, int testCaseId, int iteration, TestStepResultBlob obj);
        Task<IEnumerable<TestStepResultBlob>> GetErrorInformationBlobs(int testRunId, int testCaseId);
    }
}

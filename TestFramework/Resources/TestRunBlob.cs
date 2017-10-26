using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Shared.Common.Storage;

namespace TestFramework.Resources
{
    public class TestRunBlob : IJsonStorageEntity
    {
        public string PartKey { get; set; }
        public string RowKey { get; set; }
        public int OnlyForTestCaseWithId { get; set; }
        public string BlobStoreRef { get; set; }

        public TestRunBlob()
        {
        }

        public TestRunBlob(TestRun testRun, string rowKey)
        {
            PartKey = testRun.TestPlanId.ToString();
            RowKey = rowKey;
        }

        public string GetPartitionKey()
        {
            return PartKey;
        }

        public string GetRowKey()
        {
            return RowKey;
        }

        public TestRun GetTestRun(IBlobStorageDb blobStore)
        {
            var blobref = GetOnlyBlobId();
            var bytes = blobStore.GetBytes(blobref);
            return Deserialize<TestRun>(bytes);
        }

        private string GetOnlyBlobId()
        {
            var blobRef = BlobStoreRef;

            if (blobRef.Contains("/"))
            {
                blobRef = blobRef.Split('/').Last();
            }
            if(blobRef.Contains("?"))
            {
                blobRef = blobRef.Split('?').First();
            }

            return blobRef;
        }

        // ReSharper disable once UnusedTypeParameter
        public static TestRun Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return JsonSerializer.Create().Deserialize(reader, typeof(TestRun)) as TestRun;
                }
            }
        }
    }
}

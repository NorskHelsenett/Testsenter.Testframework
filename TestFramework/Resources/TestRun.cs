using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Shared.Common.Logic;
using Shared.Common.Testing;

namespace TestFramework.Resources
{
    public class TestRun : BaseTestRun
    {
        public string GetTestSignature()
        {
            var testAsJson = JsonConvert.SerializeObject(Tests);
            var hash = CryptoHelper.CalculateMD5Hash(testAsJson);

            return hash;
        }

        public List<Test> Tests { get; set; }

        public TestRun() { }

        public TestRun(string teamProject, int testplanId, string tfsuri, string testrunId)
            : base(teamProject, testplanId, tfsuri, testrunId)
        {
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }

        public bool AnyFailedTests()
        {
            return Tests.Any(test => test.Points.Any(point => !point.TestSteps.All(step => step.Result.Success)));
        }

        public static TestRun Load(string path)
        {
            return JsonConvert.DeserializeObject<TestRun>(File.ReadAllText(path));
        }

        public static List<TestRun> LoadAll(string path)
        {
            var list = new List<TestRun>();

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Could not find dir {path}");
            }

            foreach (var file in Directory.GetFiles(path))
            {
                var filename = Path.GetFileName(file);
                // ReSharper disable once AssignNullToNotNullAttribute
                var fullPath = Path.Combine(path, filename);
                try
                {
                    list.Add(Load(fullPath));
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not load TestRun {fullPath} - {e.Message}");
                }

            }

            return list;
        }
    }
        
}

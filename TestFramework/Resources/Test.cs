using System;
using System.Collections.Generic;
using System.Linq;
using TestFramework.Resources;

namespace Shared.Common.Testing
{
    public class Test
    {
        public Test(int testCaseId, string testCaseName, string status, string tags)
        {
            TestCaseId = testCaseId;
            Points = new List<Point>();
            Status = status;
            TestCaseName = testCaseName;
            NumberOfTries = 0;
            Tags = tags.Split(';').Select(t => t.Trim()).ToList();
        }

        public Test() { }

        public string TestCaseName { get; set; }
        public int TestCaseId { get; set; }
        public List<Point> Points { get; set; }
        public string Status { get; set; }
        public int NumberOfTries { get; set; }
        public TimeSpan ElapsedTime => new TimeSpan(Points.Select(p => p.Duration.Ticks).Sum());
        public List<string> Tags { get; set; }
        public string ExclusionGroup => Tags?.FirstOrDefault(t => t.StartsWith("Eksklusjonsgruppe"));
    }
}

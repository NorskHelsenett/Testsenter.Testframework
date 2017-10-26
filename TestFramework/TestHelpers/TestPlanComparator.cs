using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Common.Testing;

namespace TestFramework.TestHelpers
{
    public static class TestPlanComparator
    {
        public struct Result
        {
            public IEnumerable<Test> onlyInA;
            public IEnumerable<Test> onlyInB;
            public IEnumerable<Tuple<Test, Test>> common;
        }

        private static readonly string[] EnvironmentLabels = new[] { "Test01" };

        public static IEnumerable<string> GenerateReport(this Result result, string nameOfA, string nameOfB)
        {
            var report = new List<string>();

            report.Add($"\n---[ {result.onlyInA.Count()} TCs ARE ONLY IN {nameOfA} ]---");
            report.AddRange(result.onlyInA.Select(Name));

            report.Add($"\n---[ {result.onlyInB.Count()} TCs ARE ONLY IN {nameOfB} ]---");
            report.AddRange(result.onlyInB.Select(Name));

            report.Add($"\n---[ {result.common.Count()} COMMON TEST CASES ]---");
            report.Add($"<ID for {nameOfA}> - <ID for {nameOfB}> - <test case name>");
            report.AddRange(result.common.Select(tests => 
                $"{tests.Item1.TestCaseId} - {tests.Item2.TestCaseId} - {tests.Item1.TestCaseName}"));

            return report;
        }

        public static Result CompareTestPlans(IEnumerable<Test> a, IEnumerable<Test> b)
        {
            var aDict = DictionaryOf(a.ToArray());
            var bDict = DictionaryOf(b.ToArray());

            var commonTestCases = aDict.Keys.Where(name => bDict.ContainsKey(name));
            var uniquelyA = aDict.Keys.Where(name => !bDict.ContainsKey(name));
            var uniquelyB = bDict.Keys.Where(name => !aDict.ContainsKey(name));

            return new Result
            {
                onlyInA = uniquelyA.Select(k => aDict[k]),
                onlyInB = uniquelyB.Select(k => bDict[k]),
                common = commonTestCases.Select(k => Tuple.Create(aDict[k], bDict[k]))
            };
        }

        private static string Name(this Test test)
        {
            return $"{test.TestCaseId} - {test.TestCaseName}";
        }

        private static Dictionary<string, Test> DictionaryOf(Test[] tests)
        {
            return tests
                .GroupBy(t => t.TestCaseName)
                .Select(group => group.First())
                .ToDictionary(ComparableName);
        }

        private static string ComparableName(Test test)
        {
            var words = test.TestCaseName.Split('-');
            return string.Join(" - ", words.Where(w => !EnvironmentLabels.Any(l => l == w.Replace(" ", ""))));
        }
    }
}

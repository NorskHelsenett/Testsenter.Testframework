using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TestFramework.Resources;

namespace TestFramework.TestHelpers
{
    public static class TestRunComparator
    {
        public enum ResultType
        {
            RemovedTest,
            NewTest,
            ChangedTest,
            Fixed,
            ChangedError,
            NewError,
        }

        public struct ErrorMatrixLine
        {
            public int Iteration;
            public int Step;
            public string OldError;
            public string NewError;
        }

        public struct Difference
        {
            public ResultType Type;
            public string Description;
            public List<ErrorMatrixLine> ErrorMatrixDifferences;
            public int TestCaseId;
            public string TestCaseName;
        }

        public struct ComparableTest
        {
            public int TestCaseId;
            public string TestCaseName;
            public Lazy<string[][]> IterationErrors;
            public bool Passed;
            public DateTime When;
        }

        public static IEnumerable<string> GenerateReport(this IEnumerable<Difference> results)
        {
            results = results.ToArray();
            var headlines = new Dictionary<ResultType, string>
            {
                { ResultType.RemovedTest,  "Removed tests" },
                { ResultType.NewTest,      "New tests" },
                { ResultType.ChangedTest,  "Tests that were changed, and could not be compared" },
                { ResultType.Fixed,        "Tests that have been fixed" },
                { ResultType.ChangedError, "Errors that have changed error message" },
                { ResultType.NewError,     "Newly introduced errors" },
            };

            var resultGroups = from resultType in Enum.GetValues(typeof(ResultType)).Cast<ResultType>()
                               let resultOfType = results.Where(r => r.Type == resultType)
                               where resultOfType.Any()
                               select new
                               {
                                   Headline = headlines[resultType],
                                   Results = resultOfType.Select(r => $"{r.TestCaseId} - {r.TestCaseName}: {r.Description}")
                               };

            var report = new List<string>();
            foreach (var resultGroup in resultGroups)
            {
                report.Add($"---[ {resultGroup.Headline.ToUpper()} ]---");
                report.AddRange(resultGroup.Results);
                report.Add("");
            }

            return report;
        }

        public static IEnumerable<Difference> CompareTestRuns(TestRun testRunPre, TestRun testRunPost)
        {
            Func<TestRun, ComparableTest[]> convert = testRun => testRun.Tests.Select(t => new ComparableTest
            {
                TestCaseId = t.TestCaseId,
                TestCaseName = t.TestCaseName,
                Passed = t.Points.All(p => p.RunOk),
                IterationErrors = new Lazy<string[][]>(() => t.Points.OrderBy(p => p.Id).Select(p => p.TestSteps.Select(ts => ts.Result?.ErrorMessage).ToArray()).ToArray()),
                When = testRun.Finished
            }).ToArray();

            return CompareTestRuns(convert(testRunPre), convert(testRunPost));
        }

        public static IEnumerable<Difference> CompareTestRuns(ComparableTest[] testRunPre, ComparableTest[] testRunPost)
        {
            EnsureOrder(ref testRunPre, ref testRunPost);

            var testsPre = testRunPre.ToDictionary(t => t.TestCaseId);
            var testsPost = testRunPost.ToDictionary(t => t.TestCaseId);

            var commonTests = testsPre.Keys.Where(testsPost.ContainsKey);
            var removedTests = testsPre.Keys.Where(pre => !testsPost.ContainsKey(pre));
            var newTests = testsPost.Keys.Where(post => !testsPre.ContainsKey(post));

            var warnings = new List<Difference>();

            warnings.AddRange(removedTests
                .Select(id => testsPre[id])
                .Select(t => new Difference
                {
                    Type = ResultType.RemovedTest,
                    TestCaseId = t.TestCaseId,
                    TestCaseName = t.TestCaseName,
                    Description = ""
                }));

            warnings.AddRange(newTests
                .Select(id => testsPost[id])
                .Select(t => new Difference
                {
                    Type = ResultType.NewTest,
                    TestCaseId = t.TestCaseId,
                    TestCaseName = t.TestCaseName,
                    Description =  t.Passed ? "Passed" : JsonConvert.SerializeObject(t.IterationErrors.Value)
                }));


            return commonTests
                .Select(testId => CompareSingleTestRun(testsPre[testId], testsPost[testId]))
                .Where(r => r.HasValue)
                .Select(r => r.Value)
                .Concat(warnings);
        }

        private static Difference? CompareSingleTestRun(ComparableTest pre, ComparableTest post)
        {
            if (!pre.Passed && post.Passed)
            {
                return new Difference
                {
                    Type = ResultType.Fixed,
                    TestCaseId = post.TestCaseId,
                    TestCaseName = post.TestCaseName
                };
            }

            if (pre.Passed && !post.Passed)
            {
                return new Difference
                {
                    Type = ResultType.NewError,
                    TestCaseId = post.TestCaseId,
                    TestCaseName = post.TestCaseName,
                    Description = JsonConvert.SerializeObject(
                        post.IterationErrors.Value
                        .SelectMany(ie => ie.Where(e => !string.IsNullOrEmpty(e)))
                        .Select(OutputFormat))
                };
            }

            if (!pre.Passed && !post.Passed)
            {
                var stepsPre = pre.IterationErrors.Value?[0];
                var stepsPost = post.IterationErrors.Value?[0];

                if (stepsPre == null || stepsPost == null)
                {
                    var matrixRow = new ErrorMatrixLine
                    {
                        OldError = stepsPre == null ? "[error message missing in earliest run]" : "",
                        NewError = stepsPost == null ? "[error message missing in latest run]" : ""
                    };

                    return new Difference
                    {
                        Type = ResultType.ChangedError,
                        TestCaseId = post.TestCaseId,
                        TestCaseName = post.TestCaseName,
                        ErrorMatrixDifferences = new[] {matrixRow}.ToList()
                    };
                }

                if (stepsPre.Length < stepsPost.Length)
                {
                    return new Difference
                    {
                        Type = ResultType.ChangedTest,
                        TestCaseId = post.TestCaseId,
                        TestCaseName = post.TestCaseName,
                        Description = $"{stepsPost.Length - stepsPre.Length} steps have been added"
                    };
                }
                if (stepsPre.Length > stepsPost.Length)
                {
                    return new Difference
                    {
                        Type = ResultType.ChangedTest,
                        TestCaseId = post.TestCaseId,
                        TestCaseName = post.TestCaseName,
                        Description = $"{stepsPre.Length - stepsPost.Length} steps have been removed"
                    };
                }
                if (pre.IterationErrors.Value.Length < post.IterationErrors.Value.Length)
                {
                    return new Difference
                    {
                        Type = ResultType.ChangedTest,
                        TestCaseId = post.TestCaseId,
                        TestCaseName = post.TestCaseName,
                        Description = $"{post.IterationErrors.Value.Length - pre.IterationErrors.Value.Length} iterations have been added"
                    };
                }
                if (pre.IterationErrors.Value.Length > post.IterationErrors.Value.Length)
                {
                    return new Difference
                    {
                        Type = ResultType.ChangedTest,
                        TestCaseId = post.TestCaseId,
                        TestCaseName = post.TestCaseName,
                        Description = $"{pre.IterationErrors.Value.Length - post.IterationErrors.Value.Length} iterations have been removed"
                    };
                }

                var iterationErrorDifferences = DifferenceOfIterationErrors(pre.IterationErrors.Value, post.IterationErrors.Value);
                if (iterationErrorDifferences.Count > 0)
                {
                    return new Difference
                    {
                        Type = ResultType.ChangedError,
                        TestCaseId = post.TestCaseId,
                        TestCaseName = post.TestCaseName,
                        Description = null,
                        ErrorMatrixDifferences = iterationErrorDifferences
                    };
                }
            }
            return null;
        }

        private static string Trunc(this string str, int n)
        {
            return new string(str.Take(n).ToArray());
        }

        private static string OutputFormat(this string str)
        {
            return str?.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)[0].Trunc(300) ?? "[NO ERROR]";
        }

        private static string RemoveStackTrace(this string errorMsg)
        {
            if (errorMsg == null) return "[NO ERROR]";

            Func<string, int> indexOf = str => errorMsg?.ToLower().IndexOf(str) ?? -1;
            var index = new[] { "stacktrace:", "stack:" }.Select(indexOf).Min(i => i < 0 ? int.MaxValue : i);

            return index < errorMsg.Length ? errorMsg.Substring(0, index) : errorMsg;
        }

        private static string Name(this ComparableTest test)
        {
            return $"{test.TestCaseId} - {test.TestCaseName}";
        }

        private static List<ErrorMatrixLine> DifferenceOfIterationErrors(string[][] pre, string[][] post)
        {
            var res = new List<ErrorMatrixLine>();
            for (var i = 0; i < pre.Length; i++)
            {
                for (var j = 0; j < pre[0].Length; j++)
                {
                    if (pre[i][j].ComparableErrorMessage() != post[i][j].ComparableErrorMessage())
                    {
                        res.Add(new ErrorMatrixLine
                        {
                            Iteration = i,
                            Step = j,
                            OldError = pre[i][j].OutputFormat(),
                            NewError = post[i][j].OutputFormat()
                        });
                    }
                }
            }

            return res;
        }

        private static void EnsureOrder(ref ComparableTest[] pre, ref ComparableTest[] post)
        {
            if (post[0].When < pre[0].When)
            {
                var tmp = pre;
                pre = post;
                post = tmp;
            }
        }

        private static readonly Tuple<string, Regex>[] IgnorePatterns = new string[] {
            @"returnerte \d+ resultater, men forventet å få en tom liste",
            @"OpenQA\.Selenium\.WebDriverException The HTTP request to the remote WebDriver server for URL http:\/\/(127.0.0.1|localhost):\d+\/session\/[a-z\d\-]+\/timeouts timed out",
            @"(127.0.0.1|localhost):\d+",
        }.Select(pattern => Tuple.Create(pattern, new Regex(pattern))).ToArray();

        private static string ComparableErrorMessage(this string errorMsg)
        {
            if (errorMsg == null) return null;
            foreach (var ignorePattern in IgnorePatterns)
            {
                errorMsg = ignorePattern.Item2.Replace(errorMsg, ignorePattern.Item1);
            }
            return errorMsg
                .RemoveDirectoryPathFromSourceFiles()
                .RemoveStackTrace()
                .Trunc(300)
                .ToLower();
        }

        private static readonly Regex SourceFilePattern = new Regex(@"((C|D|E|F|G|H):\\[^ ]*\\([a-zA-Z0-9_æøåÆØÅ]+.cs)):line ?\d+");
        private static string RemoveDirectoryPathFromSourceFiles(this string str)
        {
            foreach (var m in SourceFilePattern.Matches(str).Cast<Match>())
            {
                str = str.Replace(m.Groups[1].Value, m.Groups[3].Value);
            }
            return str;
        }
    }
}

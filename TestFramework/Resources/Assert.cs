using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestFramework.Resources
{
    public static class Assert
    {
        public static void True(bool condition, string failMessage = "Assertion Failed: value was false, expected true")
        {
            if (!condition)
                 throw new TestCaseException(failMessage);
        }

        public static void False(bool condition, string failMessage = "Assertion Failed: value was true, expected false")
        {
            if (condition)
                throw new TestCaseException(failMessage);
        }

        public static void Fail(string failMessage)
        {
            throw new TestCaseException(failMessage);
        }

        public static void Empty<T>(IEnumerable<T> values, string description = "Forventet tom liste, men var")
        {
            values = values.ToList();
            Assert.True(values.Count() == 0, $"{description}: {JsonConvert.SerializeObject(values)}");
        }

        public static void Subset<T>(IEnumerable<T> expectedValues, IEnumerable<T> actualValues, string description = "Sammenligning feilet", Func<T, T, bool> customComparer = null)
        {
            expectedValues = expectedValues.ToList();
            actualValues = actualValues.ToList();

            True(expectedValues.All(expected => actualValues.Any(actual => Equality(expected, actual, customComparer))), 
                $"{description}: Forventet at {JsonConvert.SerializeObject(expectedValues)} skulle være et subset av {JsonConvert.SerializeObject(actualValues)}");
        }

        public static void AllEqual<T>(T expected, IEnumerable<T> actuals, string description = "Sammenligning feilet", Func<T, T, bool> customComparer = null)
        {
            actuals = actuals.ToList();
            True(actuals.All(actual => Equality(expected, actual, customComparer)),
                $"{description}: Forventet at alle skulle være lik: {JsonConvert.SerializeObject(expected)}, men var {JsonConvert.SerializeObject(actuals)}");
        }

        public static void AnyEqual<T>(T expected, IEnumerable<T> actuals, string description = "Sammenligning feilet", Func<T, T, bool> customComparer = null)
        {
            actuals = actuals.ToList();
            True(actuals.Any(actual => Equality(expected, actual, customComparer)),
                $"{description}: Forventet å finne {JsonConvert.SerializeObject(expected)}, men var {JsonConvert.SerializeObject(actuals)}");
        }

        public static void NoneEqual<T>(T notExpected, IEnumerable<T> actuals, string description = "Sammenligning feilet", Func<T, T, bool> customComparer = null)
        {
            actuals = actuals.ToList();
            False(actuals.Any(actual => Equality(notExpected, actual, customComparer)),
                $"{description}: Forventet at {JsonConvert.SerializeObject(notExpected)} ikke var i {JsonConvert.SerializeObject(actuals)}");
        }

        public static void Equal<T>(T expected, T actual, string description = "Sammenligning feilet", Func<T, T, bool> customComparer = null)
        {
            var equality = Equality(expected, actual, customComparer);
            var errorMessage = $"{description}: Forventet {JsonConvert.SerializeObject(expected)}, men var {JsonConvert.SerializeObject(actual)}";
            True(equality, errorMessage);
        }

        public static void Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual, string description = "Sammenligning feilet", Func<T, T, bool> customComparer = null)
        {
            expected = expected.OrderBy(e => e.GetHashCode()).ToArray();
            actual = actual.OrderBy(e => e.GetHashCode()).ToArray();

            var missing = expected.Where(e => !actual.Any(a => Equality(e, a, customComparer))).ToArray();
            var tooMany = actual.Where(a => !expected.Any(e => Equality(e, a, customComparer))).ToArray();

            var missingMessage = missing.Length == 0 ? "" : "Mangler følgende elementer: " + JsonConvert.SerializeObject(missing);
            var tooManyMessage = tooMany.Length == 0 ? "" : "Hadde følgende uforventede elementer: " + JsonConvert.SerializeObject(tooMany);

            if ((missingMessage + tooManyMessage).Length > 0)
            {
                Fail($"{description}: {missingMessage} {tooManyMessage}");
            }
        }

        public static void NotEqual<T>(T expected, T actual, string description = "Sammenligning feilet", Func<T, T, bool> customComparer = null)
        {
            var equality = Equality(expected, actual, customComparer);
            var errorMessage = $"{description}: Forventet at {JsonConvert.SerializeObject(expected)} ikke var lik {JsonConvert.SerializeObject(actual)}";
            False(equality, errorMessage);
        }

        private static bool Equality<T>(T expected, T actual, Func<T, T, bool> customComparer)
        {
            return customComparer?.Invoke(expected, actual) ?? EqualityComparer<T>.Default.Equals(expected, actual);
        }

        public static void NotNull(object value, string className = "Objektet")
        {
            True(value != null, $"{className} returnerte null.");
        }

        public static void IsNull(object value, string className = "Objektet")
        {
            True(value == null, $"{className} returnerte ikke null, men var {JsonConvert.SerializeObject(value)}");
        }

        public static void IsNullOrEmpty<T>(IEnumerable<T> enumerable, string className = "Objektet")
        {
            True((enumerable == null || !enumerable.Any()), $"{className} returnerte ikke null, men var {JsonConvert.SerializeObject(enumerable)}");
        }

        public static void ResultCount(int expectedCount, int actualCount, string methodName)
        {
            if (expectedCount == 0)
            {
                True(actualCount == 0, $"{methodName} returnerte {actualCount} resultater, men forventet å få en tom liste.");
            }
            else
            {
                GreaterOrEqual(actualCount, expectedCount, methodName);
            }
        }

        public static void GreaterOrEqual(int actualCount, int expectedCount, string methodName)
        {
            True(actualCount >= expectedCount,
                $"{methodName} returnerte mindre antall enn forventet. Forventet minst {expectedCount} fikk {actualCount}.");
        }

        public static void LessOrEqual(int actualCount, int expectedCount, string className)
        {
            True(actualCount <= expectedCount,
                $"{className} returnerte større antall enn forventet. Forventet i hvert fall {expectedCount} fikk {actualCount}");
        }

        public static void ThatResultIsExpected(bool condition, string action)
        {
            True(condition, $"Kunne {action} selv om det ikke var forventet");
        }

        public static void ThatCouldCompleteAction(bool condition, string action, Exception exception)
        {
            var message = $"Kunne ikke {action} selv om det var forventet: {exception.GetType()} {exception.Message}";
            True(condition, message);
        }

        public static void ThatErrorMessageIsCorrect(bool condition, string expectedError, string exceptionMessage)
        {
            var message = $"Fikk tilbake feil feilmelding. Forventet '{expectedError}', fikk '{exceptionMessage}'";
            True(condition, message);
        }

        public static void ByErrorMsg(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Fail(msg);
            }
        }

        public static void Datagrunnlag(bool condition, string message)
        {
            if (!condition) throw new ArgumentException($"Feil i datagrunnlag: {message}");
        }

        public static void WithoutStopping(Action assert, ref bool success, List<string> messages)
        {
            try
            {
                assert();
            }
            catch (TestCaseException e)
            {
                success = false;
                messages.Add(e.Message);
            }
        }

        public static void Repeatedly(Action assert, int retrySleepMs, int timeoutMs)
        {
            if (timeoutMs < 0) throw new ArgumentException("timeoutMs cannot be negative");
            var timeout = DateTime.Now.AddMilliseconds(timeoutMs);
            var failures = 0;
            while (true)
            {
                try
                {
                    assert();
                    if (failures > 0) Console.WriteLine($"Repeated assert succeeded after {failures} failed attempts.");
                    return;
                }
                catch (TestCaseException)
                {
                    var now = DateTime.Now;
                    if (now >= timeout) throw;
                    failures++;
                    Console.WriteLine($"Retrying assert in {retrySleepMs} ms. I will continue trying for another {timeout.Subtract(now).TotalSeconds} seconds.");
                    Thread.Sleep(retrySleepMs);
                }
            }
        }

        public static void JTokenNotNull(JToken jToken, string key = null, string methodUsed = null)
        {
            key = key ?? "Nøkkelen";
            Equal(JTokenType.Null, jToken.Type, $"Metoden: {methodUsed} returnerte følgende feil. {key} var ikke null, men var: {jToken}");
        }
    }
}

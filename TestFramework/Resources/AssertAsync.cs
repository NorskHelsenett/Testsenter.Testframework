using System.Threading.Tasks;

namespace TestFramework.Resources
{
    public static class AssertAsync
    {
        public static Task TrueAsync(bool condition, string failMessage = "Assertion Failed: value was true, expected false")
        {
            return Task.Run(() =>
            {
                if (!condition)
                    throw new TestCaseException(failMessage);
            });
        }
    }
}
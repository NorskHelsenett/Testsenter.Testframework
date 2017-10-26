using System;

namespace TestFramework.Resources
{
    public class TestCaseException : Exception
    {
        public TestCaseException(string message) : base(message) { }
    }
}

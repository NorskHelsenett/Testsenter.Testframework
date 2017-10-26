using System;

namespace TestFramework.Resources
{
    public class FailureToStartTestException : Exception
    {
        public FailureToStartTestException(string s, Exception e) : base(s, e) { }
    }
}

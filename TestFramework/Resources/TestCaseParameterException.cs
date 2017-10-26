using System;

namespace TestFramework.Resources
{
    public class TestCaseParameterException : Exception
    {
        public TestCaseParameterException(string key, string legalValues) 
            : base("Invalid parameter from test case: " + key + ". Legal values are " + legalValues)
        {
            
        }
    }
}
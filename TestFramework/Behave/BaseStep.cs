using System;

namespace TestFramework.Behave
{
    public class BaseStep : Attribute
    {
        public string Description;

        public BaseStep(string description)
        {
            Description = description;
        }
    }
}

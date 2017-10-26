using Shared.Common.Testing;
using System.Collections.Generic;
using System.Reflection;
using TestFramework.Resources;

namespace TestFramework.Interfaces
{
    public interface ITestInjector
    {
        void RegisterTest<TTestImplementation>(params int[] parentId) where TTestImplementation : IExecuteTestStep;
        IExecuteTestStep GetTest(TestStep s);
        IEnumerable<Assembly> GetRegisteredBehaviourdrivenAssemblies();
    }
}

using Microsoft.Practices.Unity;
using Shared.Common.Testing;
using System;
using TestFramework.Behave;
using TestFramework.Interfaces;
using TestFramework.Resources;

namespace TestFramework.Core
{
    public class TestStepFactory : ITestStepFactory
    {
        private readonly ITestInjector _injector;

        public TestStepFactory(ITestInjector di)
        {
            _injector = di;
        }

        public IExecuteTestStep GetTestStepExecutor(TestStep s)
        {
            if (s.IsSharedStep)
            {
                try
                {
                    return _injector.GetTest(s);
                }
                catch (ResolutionFailedException e)
                {
                    throw new NotImplementedException($"Could not find Implementation for TestStep: {s.ParentId} - {e.Message}");
                }
            }

            return BehavePatternStep.FindStep(_injector, s, _injector.GetRegisteredBehaviourdrivenAssemblies());
        }
    }
}

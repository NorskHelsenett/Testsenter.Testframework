using Shared.Common.DI;
using Shared.Common.Resources;
using System;
using System.Reflection;
using TestFramework.Interfaces;
using System.Collections.Generic;
using Shared.Common.Testing;
using TestFramework.Resources;

namespace TestFramework.Core
{
    public abstract class TestFrameworkDi : UnityDependencyInjector, ITestInjector
    {
        protected List<Assembly> _behaviourAssemblies;
        public Dictionary<int, string> RegisteredSharedSteps { get; set; }

        public TestFrameworkDi(ServiceDescription caller) : base(caller)
        {
            _behaviourAssemblies = new List<Assembly>();
            _behaviourAssemblies.Add(Assembly.GetExecutingAssembly());

            RegisterInstance<ITestInjector>(this);
            RegisteredSharedSteps = new Dictionary<int, string>();
        }

        public void RegisterTest<TTestImplementation>(params int[] parentId)
            where TTestImplementation : IExecuteTestStep
        {
            foreach (var id in parentId)
            {
                if(!RegisteredSharedSteps.ContainsKey(id))
                    RegisteredSharedSteps.Add(id, typeof(TTestImplementation).FullName);
                else
                    RegisteredSharedSteps[id] = typeof(TTestImplementation).FullName;

                Register<IExecuteTestStep, TTestImplementation>(InstanceLifetime.ReturnNewInstanceForEachResolve,
                    id.ToString());
            }
        }

        public IExecuteTestStep GetTest(TestStep s)
        {
            return GetInstance<IExecuteTestStep>(s.ParentId.ToString());
        }

        public void RegisterTests<TType>() where TType : ITestRegistration
        {
            ITestRegistration instance = Activator.CreateInstance<TType>();
            instance.RegisterTests(this);

            RegisterBehaviourDrivenTests<TType>();
        }

        private void RegisterBehaviourDrivenTests<TType>() where TType : ITestRegistration
        {
            var assembly = Assembly.GetAssembly(typeof(TType));
            _behaviourAssemblies.Add(assembly);
        }

        public IEnumerable<Assembly> GetRegisteredBehaviourdrivenAssemblies()
        {
            return _behaviourAssemblies;
        }

        
    }
}

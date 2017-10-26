using System;
using System.Collections.Generic;
using System.Diagnostics;
using Shared.Common.DI;

namespace TestFramework.Core
{
    public class ExternalProjectRegistrationManager
    {
        private readonly List<IExternalProjectRegistration> _externalProjectRegistrations;

        public ExternalProjectRegistrationManager()
        {
            _externalProjectRegistrations = new List<IExternalProjectRegistration>();
        }

        public void Register<T>(T instance, TestFrameworkDi di) where T : IExternalProjectRegistration
        {
            instance.Register(di, di.InstanceDescription);
            _externalProjectRegistrations.Add(instance);
        }

        public void Dispose(UnityDependencyInjector di)
        {
            foreach (var reg in _externalProjectRegistrations)
            {
                try
                {
                    reg.Dispose(di);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("While disposing, got exception: " + e.Message);
                }
            }
        }
    }
}

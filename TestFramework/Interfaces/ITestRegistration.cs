using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFramework.Interfaces
{
    public interface ITestRegistration
    {
        void RegisterTests(ITestInjector injector);
    }
}

using Shared.Common.Logic;
using TestFramework.Resources;

namespace TestFramework.Interfaces
{
    public interface IAttachmentCollector
    {
        string GetReferenceToAttachmentIfApplicable(TestState state);
    }
}

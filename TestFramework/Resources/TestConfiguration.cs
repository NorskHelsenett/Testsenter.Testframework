using System.ComponentModel;
namespace TestFramework.Resources
{
    public class TestConfiguration
    {
        public Initiator Initiator { get; set; }
        [DisplayName("Antall tråder")]
        public int NumberOfThreads { get; set; }
        public bool SuppressExceptions { get; set; }
        public int[] TestIds { get; set; }
        public string TestPlanId { get; set; }

        public Initiator GetInitiator()
        {
            return Initiator;
        }
    }

}

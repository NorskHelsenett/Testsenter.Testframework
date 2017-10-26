
namespace TestFramework.Resources
{
    public class Initiator 
    {
        public Initiator() { }

        public string WebUsername { get; set; }
        public string WebConnectionId { get; set; }
        public string SlackChannel { get; set; }
        public string SlackUsername { get; set; }
        public string SlackUserId { get; set; }
        public string SignalRListenerId { get; set; }
        public int SignalRListeningTypeInt { get; set; }

        public string FrontendCallerName { get; set; }

        public string GetCallerName()
        {
            if (!string.IsNullOrEmpty(WebUsername))
                return WebUsername;

            if(!string.IsNullOrEmpty(SlackUsername))
            {
                return SlackUsername + " (#" + SlackChannel + ")";
            }

            return "Skybert";
        }
    }
}

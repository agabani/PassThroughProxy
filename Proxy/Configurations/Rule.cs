namespace Proxy.Configurations
{
    public class Rule
    {
        public Rule(string hostname, ActionEnum action)
        {
            Hostname = hostname;
            Action = action;
        }

        public string Hostname { get; private set; }
        public ActionEnum Action { get; private set; }
    }

    public enum ActionEnum
    {
        Deny,
        Allow
    }
}
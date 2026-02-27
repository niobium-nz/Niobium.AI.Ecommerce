namespace Niobium.Ads.Analyst
{
    internal class AgentException : Exception
    {
        public AgentException(string message) : base(message)
        {
        }

        public AgentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

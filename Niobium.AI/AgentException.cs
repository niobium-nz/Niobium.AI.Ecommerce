namespace Niobium.AI
{
    public class AgentException : Exception
    {
        public AgentException(string message) : base(message)
        {
        }

        public AgentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

namespace Niobium.AI
{
    public interface IWorkflow
    {
        Task RunAsync(string conversationID, CancellationToken cancellationToken);
    }
}

namespace Niobium.AI
{
    public interface IFileStorage
    {
        Task<Uri> UploadAsync(string name, Stream stream, CancellationToken cancellationToken);
    }
}

namespace CareerTrack.Services;

public interface IFileStorageService
{
    Task SaveAsync(IFormFile file, string storedFileName, CancellationToken cancellationToken);
    Stream OpenRead(string storedFileName);
    void Delete(string storedFileName);
}

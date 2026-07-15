namespace CareerTrack.Services;

// Stocke les fichiers hors de wwwroot (App_Data/Uploads) : aucun fichier téléversé
// n'est directement accessible par URL, seul DocumentsController.Download y donne accès,
// après vérification que le document appartient à l'utilisateur courant (Règle 14).
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _rootPath = Path.Combine(environment.ContentRootPath, "App_Data", "Uploads");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task SaveAsync(IFormFile file, string storedFileName, CancellationToken cancellationToken)
    {
        string destinationPath = GetFullPath(storedFileName);
        await using FileStream destination = File.Create(destinationPath);
        await file.CopyToAsync(destination, cancellationToken);
    }

    public Stream OpenRead(string storedFileName)
    {
        return File.OpenRead(GetFullPath(storedFileName));
    }

    public void Delete(string storedFileName)
    {
        string path = GetFullPath(storedFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GetFullPath(string storedFileName)
    {
        // storedFileName est toujours un nom généré par le serveur (Guid + extension),
        // jamais dérivé d'une entrée utilisateur — GetFileName() élimine par sécurité
        // tout séparateur de chemin qui aurait pu s'y glisser.
        return Path.Combine(_rootPath, Path.GetFileName(storedFileName));
    }
}

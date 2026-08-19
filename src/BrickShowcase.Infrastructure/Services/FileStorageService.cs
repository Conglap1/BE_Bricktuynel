using BrickShowcase.Application.Interfaces;

namespace BrickShowcase.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _baseUploadFolder;

    public FileStorageService()
    {
        _baseUploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_baseUploadFolder))
        {
            Directory.CreateDirectory(_baseUploadFolder);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subFolder = "general")
    {
        // Sanitize subFolder name to prevent directory traversal
        var safeFolder = string.Join("_", subFolder.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(safeFolder)) safeFolder = "general";

        var targetDir = Path.Combine(_baseUploadFolder, safeFolder);
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var ext = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(targetDir, uniqueFileName);

        using var destinationStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(destinationStream);

        return $"/uploads/{safeFolder}/{uniqueFileName}";
    }

    public bool DeleteFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;

        // Clean leading slash if any
        var trimmedPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", trimmedPath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }

        return false;
    }
}


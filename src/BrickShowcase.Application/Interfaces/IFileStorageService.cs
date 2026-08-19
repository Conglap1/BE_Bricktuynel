namespace BrickShowcase.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subFolder = "general");
    bool DeleteFile(string relativePath);
}


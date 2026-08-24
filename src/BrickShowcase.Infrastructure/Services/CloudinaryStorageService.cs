using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using BrickShowcase.Application.Interfaces;

namespace BrickShowcase.Infrastructure.Services;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly Cloudinary? _cloudinary;
    private readonly FileStorageService _localFallbackService;

    public CloudinaryStorageService(IConfiguration configuration)
    {
        _localFallbackService = new FileStorageService();

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (!string.IsNullOrWhiteSpace(cloudName) && 
            !string.IsNullOrWhiteSpace(apiKey) && 
            !string.IsNullOrWhiteSpace(apiSecret) &&
            cloudName != "YOUR_CLOUD_NAME")
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subFolder = "general")
    {
        // Nếu chưa cấu hình Cloudinary key, tự động chuyển về lưu local (wwwroot/uploads)
        if (_cloudinary == null)
        {
            return await _localFallbackService.SaveFileAsync(fileStream, fileName, subFolder);
        }

        var safeFolder = string.Join("_", subFolder.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(safeFolder)) safeFolder = "general";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = $"brick_showcase/{safeFolder}",
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Lỗi upload Cloudinary: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    public bool DeleteFile(string relativeOrAbsoluteUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsoluteUrl)) return false;

        // Xóa ảnh cũ trên server local nếu đường dẫn bắt đầu bằng /uploads/
        if (relativeOrAbsoluteUrl.StartsWith("/uploads/") || !relativeOrAbsoluteUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return _localFallbackService.DeleteFile(relativeOrAbsoluteUrl);
        }

        // Xóa ảnh trên Cloudinary nếu đường dẫn từ Cloudinary
        if (_cloudinary != null && relativeOrAbsoluteUrl.Contains("cloudinary.com", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var publicId = ExtractPublicIdFromUrl(relativeOrAbsoluteUrl);
                if (!string.IsNullOrEmpty(publicId))
                {
                    var deletionParams = new DeletionParams(publicId);
                    var result = _cloudinary.Destroy(deletionParams);
                    return result.Result == "ok";
                }
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private static string ExtractPublicIdFromUrl(string url)
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath;
        var parts = path.Split('/');
        var uploadIndex = Array.IndexOf(parts, "upload");
        if (uploadIndex == -1 || uploadIndex >= parts.Length - 1) return string.Empty;

        var publicIdParts = parts.Skip(uploadIndex + 1).ToList();
        if (publicIdParts.Count > 0 && publicIdParts[0].StartsWith("v") && long.TryParse(publicIdParts[0].Substring(1), out _))
        {
            publicIdParts.RemoveAt(0);
        }

        var fullPathWithExt = string.Join("/", publicIdParts);
        var lastDotIndex = fullPathWithExt.LastIndexOf('.');
        return lastDotIndex > 0 ? fullPathWithExt.Substring(0, lastDotIndex) : fullPathWithExt;
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BrickShowcase.Application.Interfaces;

namespace BrickShowcase.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    private readonly IFileStorageService _fileService;

    public UploadsController(IFileStorageService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Upload ảnh vào wwwroot/uploads/{folder}
    /// Folder có thể là: products, projects, news, partners hoặc general
    /// </summary>
    [HttpPost("image")]
    [HttpPost("image/{folder}")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file, string? folder)

    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "File không hợp lệ." });
        }

        // Tối đa 10MB
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "Kích thước file không được vượt quá 10MB." });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .webp, .gif, .svg)." });
        }

        var targetFolder = !string.IsNullOrWhiteSpace(folder) ? folder : "general";

        using var stream = file.OpenReadStream();
        var relativeUrl = await _fileService.SaveFileAsync(stream, file.FileName, targetFolder);

        return Ok(new { url = relativeUrl, fileName = file.FileName, folder = targetFolder });
    }
}


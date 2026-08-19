using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BrickShowcase.Infrastructure.Data;
using BrickShowcase.Domain.Entities;
using BrickShowcase.Application.DTOs;

namespace BrickShowcase.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly BrickDbContext _db;

    public ProductsController(BrickDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] bool? activeOnly)
    {
        var query = _db.Product.Include(p => p.Images).AsQueryable();
        if (activeOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        var products = await query.OrderBy(p => p.DisplayOrder).ToListAsync();
        var dtos = products.Select(MapToDto);
        return Ok(dtos);
    }

    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<ProductDto>> GetByIdOrSlug(string idOrSlug)
    {
        Product? product = null;
        if (int.TryParse(idOrSlug, out int id))
        {
            product = await _db.Product.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        }
        else
        {
            product = await _db.Product.Include(p => p.Images).FirstOrDefaultAsync(p => p.Slug == idOrSlug);
        }

        if (product == null) return NotFound();
        return Ok(MapToDto(product));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductDto>> Create([FromBody] UpsertProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height,
            Weight = dto.Weight,
            HoleCount = dto.HoleCount,
            CompressionStrength = dto.CompressionStrength,
            WaterAbsorption = dto.WaterAbsorption,
            IsFeatured = dto.IsFeatured,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Product.Add(product);
        await _db.SaveChangesAsync();

        var imagesToSave = dto.Images?.Where(img => !string.IsNullOrWhiteSpace(img)).Distinct().ToList() ?? new List<string>();
        if (imagesToSave.Count == 0 && !string.IsNullOrWhiteSpace(dto.Image))
        {
            imagesToSave.Add(dto.Image);
        }

        for (int i = 0; i < imagesToSave.Count; i++)
        {
            _db.ProductImage.Add(new ProductImage
            {
                ProductId = product.Id,
                ImagePath = imagesToSave[i],
                IsThumbnail = i == 0,
                DisplayOrder = i + 1
            });
        }
        if (imagesToSave.Count > 0)
        {
            await _db.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetByIdOrSlug), new { idOrSlug = product.Id }, MapToDto(product));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpsertProductDto dto)
    {
        var product = await _db.Product.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        product.Name = dto.Name;
        product.Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug;
        product.ShortDescription = dto.ShortDescription;
        product.Description = dto.Description;
        product.Length = dto.Length;
        product.Width = dto.Width;
        product.Height = dto.Height;
        product.Weight = dto.Weight;
        product.HoleCount = dto.HoleCount;
        product.CompressionStrength = dto.CompressionStrength;
        product.WaterAbsorption = dto.WaterAbsorption;
        product.IsFeatured = dto.IsFeatured;
        product.DisplayOrder = dto.DisplayOrder;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        var imagesToSave = dto.Images?.Where(img => !string.IsNullOrWhiteSpace(img)).Distinct().ToList() ?? new List<string>();
        if (imagesToSave.Count == 0 && !string.IsNullOrWhiteSpace(dto.Image))
        {
            imagesToSave.Add(dto.Image);
        }

        if (imagesToSave.Count > 0)
        {
            _db.ProductImage.RemoveRange(product.Images);
            for (int i = 0; i < imagesToSave.Count; i++)
            {
                _db.ProductImage.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImagePath = imagesToSave[i],
                    IsThumbnail = i == 0,
                    DisplayOrder = i + 1
                });
            }
        }

        await _db.SaveChangesAsync();
        return Ok(MapToDto(product));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)

    {
        var product = await _db.Product.FindAsync(id);
        if (product == null) return NotFound();

        _db.Product.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ProductDto MapToDto(Product p)
    {
        var imagePaths = p.Images?.OrderBy(i => i.DisplayOrder).Select(i => i.ImagePath).ToList() ?? new List<string>();
        var thumb = p.Images?.FirstOrDefault(i => i.IsThumbnail)?.ImagePath ?? imagePaths.FirstOrDefault();
        return new ProductDto(
            p.Id,
            p.Name,
            p.Slug,
            p.ShortDescription,
            p.Description,
            p.Length,
            p.Width,
            p.Height,
            p.Weight,
            p.HoleCount,
            p.CompressionStrength,
            p.WaterAbsorption,
            p.IsFeatured,
            p.DisplayOrder,
            p.IsActive,
            thumb,
            imagePaths
        );
    }

    private static string GenerateSlug(string text)
    {
        return text.ToLowerInvariant().Replace(" ", "-").Replace("đ", "d");
    }
}

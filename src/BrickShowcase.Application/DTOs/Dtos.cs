namespace BrickShowcase.Application.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    decimal? CompressionStrength,
    decimal? FlexuralStrength,
    string? BrickGrade,
    bool IsFeatured,
    int DisplayOrder,
    bool IsActive,
    string? Image,
    List<string>? Images = null
);

public record UpsertProductDto(
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    decimal? CompressionStrength,
    decimal? FlexuralStrength,
    string? BrickGrade,
    bool IsFeatured,
    int DisplayOrder,
    bool IsActive,
    string? Image,
    List<string>? Images = null
);

public record ProjectDto(
    int Id,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? Location,
    string? CompletedDate,
    int DisplayOrder,
    bool IsFeatured,
    bool IsActive,
    string? Image,
    List<string>? Images = null
);

public record UpsertProjectDto(
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? Location,
    string? CompletedDate,
    int DisplayOrder,
    bool IsFeatured,
    bool IsActive,
    string? Image,
    List<string>? Images = null
);

public record NewsDto(
    int Id,
    string Title,
    string Slug,
    string? ThumbnailPath,
    string? Summary,
    string? Content,
    string? PublishedAt,
    bool IsActive
);

public record UpsertNewsDto(
    string Title,
    string Slug,
    string? ThumbnailPath,
    string? Summary,
    string? Content,
    string? PublishedAt,
    bool IsActive
);

public record PartnerDto(
    int Id,
    string Name,
    string? LogoPath,
    string? Website,
    int DisplayOrder,
    bool IsActive
);

public record UpsertPartnerDto(
    string Name,
    string? LogoPath,
    string? Website,
    int DisplayOrder,
    bool IsActive
);

public record ContactInfoDto(
    int Id,
    string CompanyName,
    string? Address,
    string? Phone,
    string? Email,
    string? Facebook,
    string? Zalo,
    string? Tiktok,
    string? GoogleMapEmbed,
    string? WorkingHours
);

public record ContactRequestDto(
    int Id,
    string FullName,
    string Phone,
    string? Email,
    string Content,
    bool IsRead,
    string CreatedAt
);

public record CreateContactRequestDto(
    string FullName,
    string Phone,
    string? Email,
    string Content
);

public record LoginRequestDto(
    string Username,
    string Password
);

public record LoginResponseDto(
    bool Success,
    string Token,
    string Message
);

public record ChangePasswordRequestDto(
    string Username,
    string OldPassword,
    string NewPassword
);


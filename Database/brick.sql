-- ==========================================================
-- SCRIPT CHẠY CHO DATABASE RỖNG / TẠO MỚI - BRICKCOMPANYDB
-- (Tương thích 100% với Backend .NET 9 EF Core)
-- ==========================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BrickCompanyDB')
BEGIN
    CREATE DATABASE BrickCompanyDB;
END;
GO

USE BrickCompanyDB;
GO

/* 1. AdminUser */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminUser')
BEGIN
    CREATE TABLE AdminUser (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL
    );
END;

/* 2. Product */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Product')
BEGIN
    CREATE TABLE Product (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        Slug NVARCHAR(150) NOT NULL UNIQUE,
        ShortDescription NVARCHAR(500) NULL,
        Description NVARCHAR(MAX) NULL,
        Length DECIMAL(10,2) NULL,
        Width DECIMAL(10,2) NULL,
        Height DECIMAL(10,2) NULL,
        CompressionStrength DECIMAL(10,2) NULL,
        FlexuralStrength DECIMAL(10,2) NULL,
        BrickGrade NVARCHAR(50) NULL,
        IsFeatured BIT NOT NULL DEFAULT(0),
        DisplayOrder INT NOT NULL DEFAULT(0),
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE()),
        UpdatedAt DATETIME2 NULL
    );
END;

/* 3. ProductImage */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductImage')
BEGIN
    CREATE TABLE ProductImage (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        ImagePath NVARCHAR(500) NOT NULL,
        IsThumbnail BIT NOT NULL DEFAULT(0),
        AltText NVARCHAR(255) NULL,
        DisplayOrder INT NOT NULL DEFAULT(0),
        CONSTRAINT FK_ProductImage_Product FOREIGN KEY(ProductId) REFERENCES Product(Id) ON DELETE CASCADE
    );
END;

/* 4. Project */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Project')
BEGIN
    CREATE TABLE Project (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Slug NVARCHAR(255) NOT NULL UNIQUE,
        ShortDescription NVARCHAR(500) NULL,
        Description NVARCHAR(MAX) NULL,
        Location NVARCHAR(255) NULL,
        CompletedDate DATE NULL,
        DisplayOrder INT NOT NULL DEFAULT(0),
        IsFeatured BIT NOT NULL DEFAULT(0),
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE()),
        UpdatedAt DATETIME2 NULL
    );
END;

/* 5. ProjectImage */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectImage')
BEGIN
    CREATE TABLE ProjectImage (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProjectId INT NOT NULL,
        ImagePath NVARCHAR(500) NOT NULL,
        IsThumbnail BIT NOT NULL DEFAULT(0),
        DisplayOrder INT NOT NULL DEFAULT(0),
        CONSTRAINT FK_ProjectImage_Product FOREIGN KEY(ProjectId) REFERENCES Project(Id) ON DELETE CASCADE
    );
END;

/* 6. News */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'News')
BEGIN
    CREATE TABLE News (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(250) NOT NULL,
        Slug NVARCHAR(250) NOT NULL UNIQUE,
        ThumbnailPath NVARCHAR(500) NULL,
        Summary NVARCHAR(MAX) NULL,
        Content NVARCHAR(MAX) NULL,
        PublishedAt DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE()),
        UpdatedAt DATETIME2 NULL
    );
END;

/* 7. Partner */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Partner')
BEGIN
    CREATE TABLE Partner (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        LogoPath NVARCHAR(500) NULL,
        Website NVARCHAR(255) NULL,
        DisplayOrder INT NOT NULL DEFAULT(0),
        IsActive BIT NOT NULL DEFAULT(1)
    );
END;

/* 8. ContactInfo */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactInfo')
BEGIN
    CREATE TABLE ContactInfo (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CompanyName NVARCHAR(255) NOT NULL,
        Address NVARCHAR(500) NULL,
        Phone NVARCHAR(20) NULL,
        Hotline NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Facebook NVARCHAR(255) NULL,
        Zalo NVARCHAR(255) NULL,
        Tiktok NVARCHAR(255) NULL,
        GoogleMapEmbed NVARCHAR(MAX) NULL,
        WorkingHours NVARCHAR(255) NULL
    );
END;

/* 9. ContactRequest */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactRequest')
BEGIN
    CREATE TABLE ContactRequest (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Email NVARCHAR(100) NULL,
        Content NVARCHAR(MAX) NOT NULL,
        IsRead BIT NOT NULL DEFAULT(0),
        CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE())
    );
END;

-- ==========================================================
-- SEED DỮ LIỆU BAN ĐẦU (INITIAL DATA CHUẨN KỸ THUẬT)
-- ==========================================================

/* Admin Account (admin / 01242968084Aa) */
IF NOT EXISTS (SELECT 1 FROM AdminUser WHERE Username = N'admin')
BEGIN
    INSERT INTO AdminUser (Username, PasswordHash)
    VALUES (N'admin', N'184ea4c56b9455ab63ba14510a314c1e93f7ccac6f77c41605b54f198b1ce6f5');
END;

/* Contact Info */
IF NOT EXISTS (SELECT 1 FROM ContactInfo)
BEGIN
    INSERT INTO ContactInfo (CompanyName, Address, Phone, Hotline, Email, Facebook, Zalo, Tiktok, GoogleMapEmbed, WorkingHours)
    VALUES (
        N'Công ty TNHH Gạch Thuận Lợi',
        N'KCN Mỹ Phước, Bến Cát, Bình Dương',
        N'0908 555 888',
        N'1900 1234',
        N'kinhdoanh@gachthuanloi.vn',
        N'https://facebook.com',
        N'0908555888',
        N'https://tiktok.com',
        N'https://maps.google.com/maps?q=KCN+My+Phuoc,+Ben+Cat,+Binh+Duong,+Vietnam&t=&z=14&ie=UTF8&iwloc=&output=embed',
        N'T2 – T7 · 07:30 – 17:30'
    );
END;

/* Partners */
IF NOT EXISTS (SELECT 1 FROM Partner)
BEGIN
    INSERT INTO Partner (Name, LogoPath, Website, DisplayOrder, IsActive) VALUES
    (N'Coteccons', N'/uploads/partners/coteccons.svg', N'https://coteccons.vn', 1, 1),
    (N'Hòa Bình Construction', N'/uploads/partners/hoabinh.svg', N'https://hbcg.vn', 2, 1),
    (N'Vinaconex', N'/uploads/partners/vinaconex.svg', N'https://vinaconex.com.vn', 3, 1),
    (N'Delta Group', N'/uploads/partners/deltagroup.svg', N'https://deltagroup.vn', 4, 1),
    (N'Ricons', N'/uploads/partners/ricons.svg', N'https://ricons.vn', 5, 1),
    (N'An Phong Construction', N'/uploads/partners/anphong.svg', N'https://anphong.vn', 6, 1);
END;

/* Seed Products */
IF NOT EXISTS (SELECT 1 FROM Product)
BEGIN
    INSERT INTO Product (Name, Slug, ShortDescription, Description, Length, Width, Height, CompressionStrength, FlexuralStrength, BrickGrade, IsFeatured, DisplayOrder, IsActive)
    VALUES
    (N'Gạch đất sét nung 2 lỗ (40x80x180 mm)', N'gach-dat-set-nung-2-lo-40x80x180', N'Gạch đất sét nung Tuynel 2 lỗ (gạch thẻ 2 lỗ) đạt quy chuẩn QCVN 16:2023/BXD & TCVN 6355:2009.', N'Sản phẩm gạch đất sét nung loại 2 lỗ Tuynel Thuận Lợi Mộc Hóa được sản xuất trên dây chuyền công nghệ cao, nung trong lò Tuynel liên tục ở nhiệt độ 1.050°C. Đạt chứng nhận hợp quy QCVN 16:2023/BXD, mác gạch M75, độ chịu nén vượt trội và độ hút nước thấp.', 180, 80, 40, 7.90, 1.90, N'Mác 75', 1, 1, 1),
    (N'Gạch đất sét nung 4 lỗ (80x80x180 mm)', N'gach-dat-set-nung-4-lo-80x80x180', N'Gạch đất sét nung Tuynel 4 lỗ (gạch ống 4 lỗ) đạt hợp quy QCVN 16:2023/BXD, Mác 75.', N'Gạch 4 lỗ Tuynel Thuận Lợi Mộc Hóa chuẩn kích thước 80x80x180 mm, nung lò Tuynel công nghệ nén ép đùn chân không. Độ chịu nén trung bình 7.7 - 8.1 MPa, khối lượng thể tích 0.96 g/cm³, độ hút nước 11.9 - 12.8%, tối ưu cho xây dựng tường bao và công trình dân dụng.', 180, 80, 80, 7.70, 1.80, N'Mác 75', 1, 2, 1);

    INSERT INTO ProductImage (ProductId, ImagePath, IsThumbnail, DisplayOrder)
    VALUES
    (1, N'/uploads/products/gach-the-co-lo.svg', 1, 1),
    (2, N'/uploads/products/gach-ong-lo-vuong.svg', 1, 1);
END;
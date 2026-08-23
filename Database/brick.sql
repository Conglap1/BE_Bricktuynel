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

/* 6.1 NewsSection (Mục bài viết / Câu hỏi & Trả lời) */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NewsSection')
BEGIN
    CREATE TABLE NewsSection (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        NewsId INT NOT NULL,
        Question NVARCHAR(500) NULL,
        Answer NVARCHAR(MAX) NULL,
        DisplayOrder INT NOT NULL DEFAULT(0),
        CONSTRAINT FK_NewsSection_News FOREIGN KEY(NewsId) REFERENCES News(Id) ON DELETE CASCADE
    );
END;

/* 6.2 NewsImage (Hình ảnh & Mô tả caption bài viết / mục) */
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NewsImage')
BEGIN
    CREATE TABLE NewsImage (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        NewsId INT NOT NULL,
        NewsSectionId INT NULL,
        ImagePath NVARCHAR(500) NOT NULL,
        Caption NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT(0),
        CONSTRAINT FK_NewsImage_News FOREIGN KEY(NewsId) REFERENCES News(Id) ON DELETE CASCADE,
        CONSTRAINT FK_NewsImage_NewsSection FOREIGN KEY(NewsSectionId) REFERENCES NewsSection(Id) ON DELETE NO ACTION
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
    INSERT INTO ContactInfo (CompanyName, Address, Phone, Email, Facebook, Zalo, Tiktok, GoogleMapEmbed, WorkingHours)
    VALUES (
        N'Công ty TNHH Một Thành Viên Thuận Lợi Mộc Hóa',
        N'Ấp Mới, Xã Bình Tân, Thị xã Kiến Tường, Tỉnh Long An',
        N'0918 701 472',
        N'kinhdoanh@gachthuanloi.vn',
        N'https://facebook.com',
        N'0918701472',
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

/* Seed Sample News Article with Sections & Images */
IF NOT EXISTS (SELECT 1 FROM News WHERE Slug = N'dua-robot-vao-day-chuyen-san-xuat-gach-tuynel')
BEGIN
    INSERT INTO News (Title, Slug, ThumbnailPath, Summary, Content, PublishedAt, IsActive, CreatedAt)
    VALUES (
        N'Đưa robot vào dây chuyền sản xuất gạch tuynel: Thuận Lợi từng bước tự động hóa sản xuất',
        N'dua-robot-vao-day-chuyen-san-xuat-gach-tuynel',
        N'/images/quy_trinh/B9.2 Đóng sản phẩm.jpg',
        N'Trong sản xuất vật liệu xây dựng, đặc biệt là gạch tuynel, bên cạnh chất lượng nguyên liệu và công nghệ nung, năng suất của cả dây chuyền còn phụ thuộc rất nhiều vào những công đoạn diễn ra sau khi viên gạch ra lò. Xếp gạch là một trong số đó.

Trước đây, công việc này chủ yếu dựa vào sức người. Với sản lượng lớn, việc vận chuyển và sắp xếp hàng nghìn viên gạch mỗi ngày vừa nặng nhọc, vừa tốn nhiều nhân công và khó duy trì sự đồng đều trong suốt quá trình làm việc.

Sự xuất hiện của robot xếp gạch đang tạo ra một thay đổi khá rõ trong cách vận hành các nhà máy sản xuất gạch hiện nay. Đây cũng là một trong những công đoạn được Thuận Lợi từng bước đưa vào tự động hóa trong dây chuyền sản xuất của mình.',
        N'Đưa robot vào dây chuyền sản xuất gạch tuynel: Thuận Lợi từng bước tự động hóa sản xuất',
        GETDATE(),
        1,
        GETDATE()
    );

    DECLARE @NewsId INT = SCOPE_IDENTITY();

    /* Section 1 */
    INSERT INTO NewsSection (NewsId, Question, Answer, DisplayOrder)
    VALUES (
        @NewsId,
        N'Robot xếp gạch hoạt động như thế nào?',
        N'Trong một dây chuyền sản xuất gạch tuynel, viên gạch phải trải qua nhiều công đoạn trước khi trở thành sản phẩm hoàn chỉnh, từ xử lý nguyên liệu, tạo hình, sấy, nung cho đến phân loại và xếp thành phẩm.

Khi gạch đã hoàn thành và được đưa đến vị trí tập kết, robot có thể đảm nhiệm công đoạn gắp và xếp gạch theo một chương trình được cài đặt sẵn.

Thay vì người lao động phải liên tục cúi, nâng và sắp xếp từng viên gạch, hệ thống robot thực hiện các thao tác này theo trình tự đã được thiết lập. Các viên gạch được xếp thành từng lớp, từng khối để thuận tiện cho việc lưu kho, vận chuyển và xuất hàng.

Điểm đáng chú ý của công nghệ này không nằm ở việc robot có thể làm một công việc “hiện đại” hơn con người, mà ở chỗ một công đoạn nặng và lặp lại nhiều lần được chuyển sang cho máy móc đảm nhiệm.',
        1
    );
    DECLARE @Sec1Id INT = SCOPE_IDENTITY();

    INSERT INTO NewsImage (NewsId, NewsSectionId, ImagePath, Caption, DisplayOrder)
    VALUES (@NewsId, @Sec1Id, N'/images/quy_trinh/B9.2 Đóng sản phẩm.jpg', N'Hình 1. Robot xếp gạch tự động trong dây chuyền sản xuất gạch tuynel tại Thuận Lợi.', 1);

    /* Section 2 */
    INSERT INTO NewsSection (NewsId, Question, Answer, DisplayOrder)
    VALUES (
        @NewsId,
        N'Tự động hóa giúp gì cho dây chuyền sản xuất gạch?',
        N'Đối với một cơ sở sản xuất gạch tuynel có sản lượng lớn, chỉ cần cải thiện một công đoạn cũng có thể tạo ra sự khác biệt cho cả dây chuyền.

Robot xếp gạch giúp các thao tác được thực hiện ổn định hơn, hạn chế những sai lệch do thao tác thủ công và giảm bớt việc di chuyển gạch bằng sức người. Khi cách xếp được duy trì đồng đều, quá trình tập kết và vận chuyển thành phẩm cũng thuận lợi hơn.

Một phóng sự của Long An TV về ứng dụng robot trong sản xuất gạch từng ghi nhận giải pháp robot xếp gạch có thể thay thế một lượng lớn lao động ở công đoạn này, đồng thời góp phần tăng năng suất và giảm tỷ lệ hư hao sản phẩm.

Với ngành vật liệu xây dựng, đây là những lợi ích có ý nghĩa thực tế. Bởi khi sản xuất ở quy mô lớn, hiệu quả không chỉ được tính bằng số lượng sản phẩm tạo ra mà còn nằm ở khả năng giảm hao hụt, sử dụng nhân lực hợp lý và duy trì sự ổn định của dây chuyền.',
        2
    );
    DECLARE @Sec2Id INT = SCOPE_IDENTITY();

    INSERT INTO NewsImage (NewsId, NewsSectionId, ImagePath, Caption, DisplayOrder)
    VALUES (@NewsId, @Sec2Id, N'/images/home_page.jpg', N'Hình 2. Hệ thống xếp gạch tự động trong dây chuyền sản xuất gạch tuynel tại nhà máy Thuận Lợi', 1);

    /* Section 3 */
    INSERT INTO NewsSection (NewsId, Question, Answer, DisplayOrder)
    VALUES (
        @NewsId,
        N'Robot không thay thế toàn bộ con người',
        N'Một trong những câu hỏi thường được đặt ra khi nhà máy đưa robot vào sản xuất là liệu máy móc có thay thế hoàn toàn người lao động hay không.

Thực tế, tự động hóa không đồng nghĩa với việc loại bỏ con người khỏi dây chuyền sản xuất.

Robot có thể đảm nhiệm những công việc mang tính lặp lại, nặng nhọc và yêu cầu thao tác liên tục. Trong khi đó, con người vẫn giữ vai trò quan trọng trong việc vận hành thiết bị, kiểm tra hoạt động của dây chuyền, xử lý tình huống và kiểm soát chất lượng sản phẩm.

Cách tiếp cận này giúp doanh nghiệp tận dụng thế mạnh của cả máy móc và con người: máy móc làm tốt những công việc cần sự lặp lại và ổn định, còn con người tập trung vào những công việc cần quan sát, xử lý và đưa ra quyết định.',
        3
    );

    /* Section 4 */
    INSERT INTO NewsSection (NewsId, Question, Answer, DisplayOrder)
    VALUES (
        @NewsId,
        N'Từ một công đoạn đến cả dây chuyền sản xuất gạch tuynel',
        N'Đưa robot vào công đoạn xếp gạch chỉ là một phần trong quá trình tự động hóa nhà máy.

Để một dây chuyền sản xuất gạch tuynel hoạt động hiệu quả, các công đoạn phải được kết nối với nhau một cách đồng bộ. Từ khâu chuẩn bị nguyên liệu, tạo hình viên gạch, sấy, nung cho đến phân loại, xếp và xuất hàng, mỗi bước đều có ảnh hưởng đến chất lượng và năng suất cuối cùng.

Chính vì vậy, tự động hóa không nên được nhìn như việc bổ sung một vài thiết bị riêng lẻ. Quan trọng hơn là cách các thiết bị phối hợp với nhau để tạo thành một dây chuyền sản xuất ổn định.

Đây cũng là hướng đi mà Thuận Lợi đang từng bước theo đuổi: đưa công nghệ vào những công đoạn phù hợp, giảm bớt những phần việc nặng, tối ưu quy trình và nâng cao hiệu quả sản xuất gạch tuynel.',
        4
    );

    /* Section 5 */
    INSERT INTO NewsSection (NewsId, Question, Answer, DisplayOrder)
    VALUES (
        @NewsId,
        N'Công nghệ thay đổi cách sản xuất vật liệu xây dựng',
        N'Ngành vật liệu xây dựng vốn có lịch sử lâu đời, nhưng cách sản xuất không vì thế mà đứng yên.

Nhu cầu về gạch xây dựng ngày càng đòi hỏi doanh nghiệp phải kiểm soát tốt hơn về chất lượng, năng suất, tiến độ giao hàng và chi phí sản xuất. Khi quy mô sản xuất tăng lên, việc tiếp tục phụ thuộc hoàn toàn vào phương pháp thủ công sẽ ngày càng khó đáp ứng.

Đó là lý do tự động hóa đang trở thành một phần quan trọng trong quá trình hiện đại hóa các nhà máy vật liệu xây dựng.

Robot xếp gạch là một ví dụ dễ thấy. Từ một công việc vốn cần nhiều sức người, nay máy móc có thể hỗ trợ thực hiện với tính ổn định cao hơn. Nhưng phía sau đó vẫn là cả một hệ thống dây chuyền, đội ngũ vận hành và quá trình kiểm soát chất lượng.',
        5
    );

    /* Section 6 */
    INSERT INTO NewsSection (NewsId, Question, Answer, DisplayOrder)
    VALUES (
        @NewsId,
        N'Thuận Lợi và định hướng sản xuất hiện đại',
        N'Với Thuận Lợi, việc đầu tư vào tự động hóa không chỉ nhằm tăng mức độ hiện đại của nhà máy. Điều quan trọng hơn là tạo ra một quy trình sản xuất phù hợp với thực tế, trong đó công nghệ hỗ trợ con người và giúp từng công đoạn vận hành hiệu quả hơn.

Gạch tuynel vẫn là sản phẩm quen thuộc trong nhiều công trình xây dựng. Nhưng để tạo ra một sản phẩm ổn định, phía sau mỗi viên gạch là cả một dây chuyền với nhiều công đoạn liên kết chặt chẽ.

Vì vậy, với Thuận Lợi, tự động hóa không phải là một câu chuyện xa vời. Nó bắt đầu từ chính những công việc cụ thể trong nhà máy: một công đoạn được cải tiến, một thiết bị được đầu tư, một quy trình được tối ưu và từng bước kết nối lại thành một dây chuyền sản xuất hiệu quả hơn.

Robot xếp gạch chỉ là một phần trong quá trình đó. Phía trước vẫn còn nhiều công đoạn có thể tiếp tục được cải tiến để hướng đến một dây chuyền sản xuất gạch tuynel ngày càng ổn định, hiệu quả và phù hợp hơn với yêu cầu của ngành vật liệu xây dựng hiện nay.',
        6
    );
END;
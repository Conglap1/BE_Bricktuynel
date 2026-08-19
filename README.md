# BE_Bricktuynel - Backend Service (.NET 9)

Hệ thống Backend cho dự án Bricktuynel được viết bằng .NET 9 Web API.

## Cấu trúc thư mục
- `src/BrickShowcase.Api`: API Controller & Endpoints.
- `src/BrickShowcase.Application`: Business Logic, DTOs, Services.
- `src/BrickShowcase.Domain`: Entities, Value Objects, Domain Logic.
- `src/BrickShowcase.Infrastructure`: Database Context, Repositories, External Services.
- `Database/`: File cấu hình và script cơ sở dữ liệu.
- `.github/workflows/deploy-azure.yml`: File cấu hình CI/CD tự động deploy lên Azure App Service.

## Hướng dẫn chạy cục bộ (Local Development)
1. **Yêu cầu**: .NET 9 SDK, SQL Server hoặc Docker.
2. **Khởi chạy API**:
   ```bash
   dotnet restore
   dotnet run --project src/BrickShowcase.Api/BrickShowcase.Api.csproj
   ```
3. **Khởi chạy bằng Docker**:
   ```bash
   docker build -t be-bricktuynel .
   docker run -p 8080:8080 be-bricktuynel
   ```

## Cấu hình CI/CD với Azure
Pipeline tự động build & deploy được đặt trong `.github/workflows/deploy-azure.yml`.

Cần bổ sung Secret trên GitHub Repository (`Settings -> Secrets and variables -> Actions`):
- `AZURE_WEBAPP_PUBLISH_PROFILE`: Nội dung Publish Profile tải về từ Azure App Service.

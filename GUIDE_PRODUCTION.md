# 🚀 Hướng Dẫn Deploy & Cấu Hình Production: Vercel (FE) + Azure App Service (BE) + Azure SQL Database (DB)

Tài liệu này được biên soạn chuẩn xác 100% cho mô hình hạ tầng Cloud PaaS bạn đang sử dụng:
- **Frontend (FE)**: Host trên **Vercel**
- **Backend (BE)**: Host trên **Azure App Service** (.NET 9 Web App)
- **Database (DB)**: **Azure SQL Database**

---

## 📌 1. Cấu Hình Frontend Lên Vercel

### Bước 1: Deploy lên Vercel
1. Truy cập [Vercel Dashboard](https://vercel.com/dashboard) -> **Add New Project**.
2. Kết nối với Git Repository của bạn và chọn thư mục root của Frontend: `FE SRC/Corporate Showcase Website Design`.
3. **Framework Preset**: Chọn `Vite`.

### Bước 2: Khai báo Biến Môi Trường (Environment Variables) trên Vercel
Tại mục **Environment Variables** trong Vercel, thêm biến sau:
- **Key**: `VITE_API_BASE_URL`
- **Value**: `https://gach-thuanloi-api-aqesccf9cjgde9ep.eastasia-01.azurewebsites.net/api` *(Thay bằng URL App Service của bạn)*

### Bước 3: Xử lý SPA Routing
File [`vercel.json`](file:///var/home/CLap/Projects/webgach/FE%20SRC/Corporate%20Showcase%20Website%20Design/vercel.json) đã được tạo sẵn trong dự án với nội dung:
```json
{
  "rewrites": [
    { "source": "/(.*)", "destination": "/index.html" }
  ]
}
```
*File này đảm bảo khi người dùng F5 hoặc truy cập trực tiếp các đường dẫn như `/san-pham`, `/tin-tuc`, `/admin` không bị lỗi 404.*

---

## 🔒 2. Cấu Hình Bảo Mật Trên Azure App Service (Backend)

Truy cập [Azure Portal](https://portal.azure.com) -> Tìm đến **App Service** của bạn (`gach-thuanloi-api`).

### Bước 1: Khai báo Application Settings (Biến Môi Trường)
Vào **Settings** -> **Environment variables** (hoặc **Configuration** -> **Application settings**):

Thêm các key sau:
1. `ConnectionStrings__DefaultConnection`:
   - **Value**: `Server=tcp:your-sql-server.database.windows.net,1433;Initial Catalog=BrickCompanyDB;Persist Security Info=False;User ID=your_sql_user;Password=your_sql_password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
2. `Jwt__SecretKey`:
   - **Value**: Chuỗi bí mật ngẫu nhiên dài trên 32 ký tự (Ví dụ: `SuperSecretKeyForBrickShowcaseApiProductionEnvironment2026!`)
3. `Jwt__Issuer`: `BrickShowcaseApi`
4. `Jwt__Audience`: `BrickShowcaseClient`
5. `AllowedOrigins`:
   - **Value**: `https://your-app-name.vercel.app,https://gachthuanloi.vn` *(Điền domain Vercel và domain chính thức của bạn để khóa CORS)*

### Bước 2: Bắt Buộc HTTPS
Vào **Settings** -> **TLS/SSL settings** -> Bật `HTTPS Only` = **On**. Mọi kết nối HTTP thường sẽ tự động chuyển hướng mã hóa HTTPS.

---

## 🛡️ 3. Bảo Mật Azure SQL Database

1. Truy cập [Azure Portal](https://portal.azure.com) -> **SQL databases** -> Chọn database `BrickCompanyDB`.
2. Chọn **Set server firewall**:
   - Tích chọn **"Allow Azure services and resources to access this server" = Yes** (Để Azure App Service có thể truy cập Database).
   - Khóa toàn bộ các dải IP public khác (không cho truy cập tự do từ bên ngoài).
3. **Sao lưu tự động (Automated Backup)**: Azure SQL Database đã tích hợp sẵn tính năng tự động sao lưu **Point-in-Time Restore (PITR)** 7 - 35 ngày. Bạn không cần thiết lập thủ công.

---

## 📤 4. Deploy Code Backend Đã Gia Cố Lên Azure App Service

Bạn có thể publish bản mã nguồn mới (đã sửa bảo mật, Rate Limiting, PBKDF2, Authorize) lên Azure bằng các cách sau:

### Cách 1: Sử dụng Visual Studio / VS Code (Khuyên dùng)
1. Mở solution `BrickShowcase.sln` trong Visual Studio.
2. Chuột phải project `BrickShowcase.Api` -> Chọn **Publish**.
3. Chọn target là **Azure App Service (Windows/Linux)** -> Select Web App `gach-thuanloi-api` -> Bấm **Publish**.

### Cách 2: Deploy bằng lệnh CLI (Zip Deploy)
```bash
# 1. Build release
dotnet publish src/BrickShowcase.Api/BrickShowcase.Api.csproj -c Release -o ./publish

# 2. Nén thư mục publish thành app.zip
cd publish && zip -r ../app.zip . && cd ..

# 3. Deploy lên Azure qua Azure CLI
az webapp deployment source config-zip --resource-group YOUR_RESOURCE_GROUP --name gach-thuanloi-api --src app.zip
```

---

## 🔑 5. Thay Đổi Mật Khẩu Admin Lần Đầu

Mật khẩu admin khởi tạo ban đầu trong database là: `Admin123@2026` (hoặc mật khẩu admin mặc định).

1. Đăng nhập trang Admin trên Vercel Frontend.
2. Thực hiện đổi mật khẩu sang mật khẩu mới.
3. Backend .NET 9 sẽ tự động băm mật khẩu theo chuẩn **PBKDF2 + Salt ngẫu nhiên (100.000 vòng lặp)** và lưu an toàn vào Azure SQL DB.

---

## ✅ Danh Sách Đánh Giá An Ninh Đã Hoàn Thành (PaaS Checklist)

- [x] Backend đã gia cố Path Traversal protection trong upload file.
- [x] Phân quyền `[Authorize]` chặt chẽ cho toàn bộ API nhạy cảm.
- [x] Khóa CORS chỉ cho phép Vercel Domain & Domain chính thức.
- [x] Tích hợp Rate Limiting chống Brute Force & Spam Request.
- [x] Tạo file `vercel.json` hỗ trợ SPA Routing mượt mà trên Vercel.
- [x] Bật HTTPS Only & mã hóa kết nối Encrypt=True tới Azure SQL.

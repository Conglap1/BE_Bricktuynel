#!/usr/bin/env bash
# ==============================================================================
# Script tự động triển khai Backend .NET 9 + PostgreSQL lên Cloud VPS
# ==============================================================================

set -e

VPS_IP="${1}"
VPS_USER="${2:-root}"

if [ -z "$VPS_IP" ]; then
    echo "Cách sử dụng: ./deploy_vps.sh <IP_CỦA_VPS> [USER_MAC_DINH_LA_ROOT]"
    echo "Ví dụ: ./deploy_vps.sh 103.179.188.50"
    exit 1
fi

echo "=========================================================="
echo "🚀 BẮT ĐẦU TRIỂN KHAI BACKEND LÊN VPS: $VPS_USER@$VPS_IP"
echo "=========================================================="

# 1. Cài đặt Docker & cấu hình môi trường trên VPS qua SSH
echo "📦 Bước 1: Kiểm tra và cài đặt Docker trên VPS..."
ssh -o StrictHostKeyChecking=no "$VPS_USER@$VPS_IP" bash << 'EOF'
set -e
export DEBIAN_FRONTEND=noninteractive

# Update system
apt-get update -y && apt-get upgrade -y

# Install prerequisite packages
apt-get install -y ca-certificates curl gnupg lsb-release ufw rsync

# Install Docker if not present
if ! command -v docker &> /dev/null; then
    echo "--> Cài đặt Docker Engine..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    rm -f get-docker.sh
fi

# Enable and start Docker
systemctl enable docker
systemctl start docker

# Configure Firewall (Chỉ mở các port web an toàn, ĐÓNG cổng Database 5432 khỏi Internet)
ufw allow 22/tcp || true
ufw allow 80/tcp || true
ufw allow 443/tcp || true
ufw allow 5247/tcp || true
ufw --force enable || true

# Prepare project directory
mkdir -p /opt/webgach
EOF

# 2. Đồng bộ mã nguồn lên VPS
echo "📤 Bước 2: Đẩy mã nguồn dự án lên /opt/webgach trên VPS..."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

rsync -avz --exclude 'bin' --exclude 'obj' --exclude '.git' --exclude '.vs' --exclude 'FE SRC' \
    -e "ssh -o StrictHostKeyChecking=no" \
    "$SCRIPT_DIR/src" \
    "$SCRIPT_DIR/Database" \
    "$SCRIPT_DIR/BrickShowcase.sln" \
    "$SCRIPT_DIR/Dockerfile.backend" \
    "$SCRIPT_DIR/docker-compose.yml" \
    "$SCRIPT_DIR/.env" \
    "$VPS_USER@$VPS_IP:/opt/webgach/"

# 3. Khởi động Container trên VPS
echo "🔨 Bước 3: Build và khởi động các container (Backend .NET 9 + PostgreSQL 16)..."
ssh -o StrictHostKeyChecking=no "$VPS_USER@$VPS_IP" bash << 'EOF'
set -e
cd /opt/webgach

# Dừng và xóa container cũ nếu có
docker compose down || true

# Khởi động dịch vụ
docker compose up -d --build

echo "⏳ Đang đợi Database PostgreSQL khởi động hoàn tất..."
sleep 10

# Database sẽ được giữ rỗng sạch sẽ, sẵn sàng chờ bạn nạp dữ liệu thật từ Azure vào sau
echo "✨ Database PostgreSQL rỗng và sạch sẽ đã sẵn sàng cho dữ liệu thật."

echo "✅ Kiểm tra trạng thái các container:"
docker compose ps
EOF

echo ""
echo "=========================================================="
echo "🎉 TRIỂN KHAI THÀNH CÔNG!"
echo "API Backend của bạn đã hoạt động tại: http://$VPS_IP:5247/api"
echo "Hãy cập nhật biến VITE_API_BASE_URL trên Vercel thành: http://$VPS_IP:5247/api"
echo "=========================================================="

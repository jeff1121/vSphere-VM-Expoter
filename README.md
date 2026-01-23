# vSphere VM Exporter

**版本**: 8.3.0

這是一個 Web 應用程式，旨在協助使用者從 VMware vSphere 匯出虛擬機 (VM) 為 OVA 格式，並將其儲存至 Minio Object Storage。

## 功能特色

*   **vSphere 整合**: 支援 vSphere 8+，可登入並列出 VM。
*   **一鍵匯出**: 選擇 VM 後自動匯出為 OVA (若 VM 為開機狀態，將自動執行關機)。
*   **Minio 儲存**: 匯出的檔案自動上傳至 Minio S3 相容儲存。
*   **即時進度**: 網頁介面顯示匯出與上傳進度。
*   **下載連結**: 完成後提供直接下載連結。

## 技術堆疊

*   **Backend**: .NET 8 (C#)
*   **Frontend**: Vue 3 + Vuetify
*   **Storage**: Minio
*   **Container**: Docker

## 開發原則

本專案遵循 **MVP (最小可行性產品)** 原則，避免過度設計。詳細開發規範請參閱 [CONSTITUTION.md](CONSTITUTION.md)。

## 專案文件

*   [SPECIFICATION.md](SPECIFICATION.md): 功能規格書
*   [PLAN.md](PLAN.md): 實作計畫
*   [TASKS.md](TASKS.md): 任務清單

## 快速開始

### 1. 前置準備 (OVF Tool)

本專案需要 VMware OVF Tool 才能執行真實的匯出功能。由於授權限制，您必須自行下載。

1.  前往 VMware 網站下載 **Linux 64-bit** 版本的 OVF Tool (Zip 或 Bundle 皆可，本專案使用解壓縮後的檔案)。
2.  將下載的檔案解壓縮。
3.  將解壓縮後的 `ovftool` 資料夾內容放置於專案的 `docs/ovftool/` 目錄下。
    *   確認路徑存在: `docs/ovftool/ovftool` (執行檔)

### 2. 建置 Docker 映像檔

本專案將建置與執行設定檔分開，以確保生產環境的單純性。使用 `docker-compose.build.yml` 來建置後端與前端的映像檔：

```bash
docker-compose -f docker-compose.build.yml build
```

### 3. 啟動服務

使用 `docker-compose.yml` 來啟動所有服務：

```bash
docker-compose up -d
```

### 4. UI 品牌識別 (可選)

您可以替換預設的 Logo 與 Favicon：

*   **Logo**: 將您的圖片 (支援 PNG/JPG) 放置於 `frontend/src/assets/logo.jpg` (或 `.png`，需修改程式碼)。
*   **Favicon**: 將您的圖示放置於 `frontend/public/favicon.png`。

若更換圖片，請重新執行步驟 2 與 3 以重建映像檔。

### 5. 存取應用程式

*   **Frontend (Web UI)**: [http://localhost:4173](http://localhost:4173)
*   **Backend (Swagger)**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
*   **Minio Console**: [http://localhost:9001](http://localhost:9001) (User/Pass: `minioadmin`)

## 開發與除錯

*   **前端路由**: 前端容器使用 Nginx 服務，並已設定 `try_files` 以支援 Vue Router 的 History Mode (避免重新整理產生 404)。
*   **查看後端日誌** (包含 OVF Tool 輸出):
    ```bash
    docker logs -f vsphere-backend
    ```
*   **重建特定服務** (例如修改前端後):
    ```bash
    docker-compose -f docker-compose.build.yml build frontend
    docker-compose up -d frontend
    ```

# 實作計畫 (Implementation Plan): vSphere VM Exporter

**分支**: `main` | **日期**: 2025-12-10 | **規格**: [SPECIFICATION.md](SPECIFICATION.md) | **版本**: 8.3.0
**輸入**: 基於 SPECIFICATION.md 的功能需求與使用者技術選型。

## 摘要

開發一個 Web 應用程式，允許使用者登入 vCenter，選擇虛擬機 (VM)，將其匯出為 OVA 格式並上傳至 Minio Object Storage。前端提供即時進度顯示與下載連結。

## 技術環境 (Technical Context)

*   **後端語言/框架**: .NET 8 (C#) - ASP.NET Core Web API
*   **前端語言/框架**: Vue 3 (Composition API) + Vuetify (UI Component Library)
*   **主要依賴**:
    *   **Backend**: VMware vSphere Web Services SDK / API, Minio Client SDK for .NET
    *   **Frontend**: Axios (API Client), Pinia (State Management)
*   **儲存**: Minio Object Storage (S3 Compatible)
*   **測試**: xUnit (Backend), Vitest (Frontend)
*   **目標平台**: 跨平台 (Windows/Linux/macOS via .NET Core), 容器化部署 (Docker)
*   **開發原則**: MVP (最小可行性產品), 不過度設計
*   **語言設定**: 程式碼註解與文件使用 **繁體中文 (zh-tw)**

## 專案結構 (Project Structure)

採用前後端分離架構。

```text
vSphere-VM-Expoter/
├── backend/                # .NET 8 Web API
│   ├── src/
│   │   ├── Controllers/    # API 端點
│   │   ├── Services/       # 業務邏輯 (vSphereService, MinioService)
│   │   ├── Models/         # 資料模型 (DTOs)
│   │   └── Program.cs      # 應用程式入口與 DI 設定
│   └── tests/              # 單元測試
├── frontend/               # Vue 3 + Vuetify
│   ├── src/
│   │   ├── components/     # UI 元件 (VmList, ExportProgress)
│   │   ├── views/          # 頁面 (Login, Dashboard)
│   │   ├── stores/         # 狀態管理 (AuthStore, TaskStore)
│   │   └── services/       # API 呼叫封裝
│   └── public/
├── docker-compose.yml      # 開發環境編排 (Minio, Backend, Frontend)
├── CONSTITUTION.md         # 開發原則
├── SPECIFICATION.md        # 功能規格
└── PLAN.md                 # 本計畫文件
```

## 開發階段 (Development Phases)

### 階段 0: 環境建置與專案初始化 (Phase 0: Setup)
1.  初始化 Git Repository (確保 Commit Message 詳細)。
2.  建立 .NET 8 Web API 專案結構。
3.  建立 Vue 3 + Vuetify 專案結構。
4.  設定 Docker Compose (包含 Minio 服務以便本地開發)。

### 階段 1: 後端核心開發 (Phase 1: Backend Core)
1.  **vSphere 整合**:
    *   實作 `IVSphereService`。
    *   功能: 登入驗證 (Login)、取得 VM 列表 (GetVMs)。
    *   **匯出實作**: 整合 VMware OVF Tool (CLI) 進行真實匯出 (ExportVm)。
    *   *注意: 需處理 vSphere 8 相容性。*
2.  **Minio 整合**:
    *   實作 `IMinioService`。
    *   功能: 上傳檔案 (UploadFile)、產生下載連結 (GetPresignedUrl)。
3.  **任務管理**:
    *   實作匯出任務的狀態追蹤 (In-Memory 即可，MVP 不需資料庫)。
    *   提供 API 查詢任務進度。

### 階段 2: 前端介面開發 (Phase 2: Frontend UI)
1.  **登入頁面**:
    *   表單: Host, Username, Password。
    *   整合後端登入 API。
2.  **VM 列表與儀表板**:
    *   使用 Vuetify Data Table 顯示 VM 資訊。
    *   加入 "匯出" 按鈕。
3.  **匯出流程與進度**:
    *   點擊匯出後顯示進度條 (Progress Bar)。
    *   輪詢 (Polling) 後端 API 取得進度。
4.  **下載**:
    *   任務完成後顯示下載按鈕。

### 階段 3: 整合與測試 (Phase 3: Integration & Testing)
1.  **端對端測試**: 模擬完整流程 (登入 -> 列表 -> 匯出 -> 下載)。
2.  **錯誤處理**: 測試 vCenter 連線失敗、Minio 斷線等情境。
3.  **UI 優化**: 確保 Vuetify 元件在不同解析度下顯示正常。

### 階段 4: 優化與部署準備 (Phase 4: Refinement & Deployment)
1.  **基礎設施重構**: 將 `docker-compose` 拆分為建置 (Build) 與執行 (Runtime) 設定，優化生產環境部署。
2.  **前端路由修復**: 引入 Nginx 處理 SPA 路由 (History Mode)，解決重新整理 404 問題。
3.  **狀態持久化**: 實作 Pinia LocalStorage 持久化，防止重新整理後登入狀態遺失。
4.  **品牌識別**: 加入 Logo 與 Favicon 支援。

## 規範檢查 (Constitution Check)

*   **MVP**: 是否包含非必要功能? -> *否，僅包含登入、列表、匯出、下載。*
*   **Code Quality**: 確保所有 Service 與複雜邏輯皆有繁體中文註解。
*   **Git**: Commit 時需遵循 "type: subject \n\n body" 格式，詳細描述變更。

## 下一步 (Next Steps)

執行 `/speckit.tasks` 將計畫轉換為具體的開發任務清單。

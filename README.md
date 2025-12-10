# vSphere VM Exporter

這是一個 Web 應用程式，旨在協助使用者從 VMware vSphere 匯出虛擬機 (VM) 為 OVA 格式，並將其儲存至 Minio Object Storage。

## 功能特色

*   **vSphere 整合**: 支援 vSphere 8+，可登入並列出 VM。
*   **一鍵匯出**: 選擇 VM 後自動匯出為 OVA。
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

(待補充: Docker Compose 啟動指令)

# 功能規格書: vSphere VM Exporter Web App

**功能分支**: `feature/vm-exporter-webapp`
**建立日期**: 2025-12-10
**狀態**: 草稿
**輸入**: 使用者需求描述: "建立一個 Web APP 可以協助使用者 從 vmware vsphere export 目標虛擬機出來成 {vm-name}.ova"

## 使用者情境與測試 (User Scenarios & Testing)

### 使用者故事 1 - vCenter 登入與 VM 列表 (優先級: P1)

使用者進入 Web App 首頁，輸入 vCenter 連線資訊 (Host, User, Password)，成功登入後看到可用的 VM 列表。

**為何此優先級**: 這是所有後續操作的基礎，無法登入或看不到 VM 就無法進行匯出。

**獨立測試**: 啟動 Web App，輸入有效的 vSphere 8+ 憑證，驗證是否能成功連線並顯示 VM 清單。

**驗收情境**:

1. **Given** 使用者在首頁, **When** 輸入正確的 vCenter Host, User, Password 並點擊登入, **Then** 系統顯示該 vCenter 下的 VM 列表。
2. **Given** 使用者在首頁, **When** 輸入錯誤的憑證, **Then** 系統顯示清晰的錯誤訊息 (例如: "帳號或密碼錯誤", "無法連線至 Host")。

---

### 使用者故事 2 - 匯出 VM 並上傳至 Minio (優先級: P1)

使用者從列表中選擇一個 VM，點擊匯出。系統將 VM 匯出為 OVA 格式並儲存至 Minio Object Storage。

**為何此優先級**: 這是核心價值功能 (MVP)，實現將 VM 取出並存儲的目標。

**獨立測試**: 選擇一個小型 VM 進行匯出，驗證 Minio Bucket 中是否出現對應的 `.ova` 檔案。

**驗收情境**:

1. **Given** 使用者已登入並看到 VM 列表, **When** 選擇一個 VM 並點擊 "Export", **Then** 系統開始匯出流程。
2. **Given** 匯出流程完成, **When** 檢查 Minio Storage, **Then** 存在名為 `{vm-name}.ova` 的檔案。

---

### 使用者故事 3 - 取得下載連結 (優先級: P1)

匯出完成後，Web App 介面上顯示該 OVA 檔案的下載連結，使用者點擊即可下載。

**為何此優先級**: 完成 "Export" 的最後一哩路，讓使用者能取得檔案。

**獨立測試**: 模擬匯出完成的狀態，驗證生成的連結是否可被瀏覽器存取並觸發下載。

**驗收情境**:

1. **Given** VM 匯出並上傳 Minio 成功, **When** 流程結束, **Then** 畫面上顯示 "下載 {vm-name}.ova" 的按鈕或連結。
2. **Given** 顯示下載連結, **When** 點擊連結, **Then** 瀏覽器開始下載 `.ova` 檔案。

---

### 使用者故事 4 - 即時進度顯示 (優先級: P2)

在匯出過程中，網頁動態更新顯示目前的進度 (例如百分比或狀態文字)，讓使用者知道系統正在運作。

**為何此優先級**: 提升使用者體驗 (UX)，避免使用者在長時間等待中以為系統當機，但非核心功能 (P1) 的絕對必要條件。

**獨立測試**: 觸發一個較長時間的匯出任務，觀察 UI 是否定期更新狀態。

**驗收情境**:

1. **Given** 正在進行匯出, **When** 等待過程中, **Then** 畫面顯示 "匯出中... xx%" 或類似的動態狀態指示。

### 邊界案例 (Edge Cases)

- **vCenter 版本不支援**: 當使用者嘗試連線低於 vSphere 8 的版本時，系統應拒絕連線並提示版本不支援。
- **Minio 空間不足**: 當 Minio 儲存空間不足以存放 OVA 時，匯出流程應提早失敗並通知使用者。
- **VM 名稱包含特殊字元**: 確保匯出的檔案名稱經過處理，符合 Minio 物件命名規則及作業系統檔案命名規則。
- **網路中斷**: 匯出過程中若 vCenter 或 Minio 連線中斷，應顯示失敗並允許重試。

## 需求 (Requirements)

### 功能需求 (Functional Requirements)

- **FR-001**: 系統必須提供 Web 介面供使用者操作。
- **FR-002**: 系統必須整合 Minio Object Storage 作為檔案儲存後端。
- **FR-003**: 系統必須支援使用者輸入 vCenter Address, Username, Password 進行動態連線 (不儲存於後端設定檔)。
- **FR-004**: 系統必須支援 vSphere 8 或以上版本。
- **FR-005**: 系統必須能列出 vCenter 中的虛擬機 (VM List)。
- **FR-006**: 系統必須能將選定的 VM 匯出為 `.ova` 格式。
- **FR-007**: 系統必須將匯出的 `.ova` 檔案上傳至 Minio。
- **FR-008**: 系統必須在匯出完成後產生 Minio 的下載連結 (Presigned URL 或公開連結)。
- **FR-009**: 系統必須在前端顯示匯出任務的即時進度。

### 關鍵實體 (Key Entities)

- **ExportTask**: 代表一個匯出任務，包含 來源 VM ID、目標 Minio Bucket/Key、目前狀態 (Pending, Running, Completed, Failed)、進度百分比。
- **VsphereSession**: 代表使用者與 vCenter 的連線階段 (Session)。

## 成功標準 (Success Criteria)

### 可測量成果 (Measurable Outcomes)

- **SC-001**: 使用者能在 3 分鐘內完成從登入到觸發匯出的操作流程。
- **SC-002**: 匯出的 `.ova` 檔案經測試可成功匯入回 vSphere 或其他相容環境 (驗證檔案完整性)。
- **SC-003**: 系統能正確處理 10GB 以上的 VM 匯出而不發生記憶體溢出 (OOM)。
- **SC-004**: 支援 vSphere 8.0 環境的連線與操作。

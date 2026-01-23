---
description: "Task list for vSphere VM Exporter implementation"
---

# Tasks: vSphere VM Exporter

**版本**: 8.3.0
**Input**: Design documents from `SPECIFICATION.md` and `PLAN.md`
**Prerequisites**: `PLAN.md` (required), `SPECIFICATION.md` (required for user stories)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)

## Path Conventions

- **Backend**: `backend/src/`, `backend/tests/`
- **Frontend**: `frontend/src/`, `frontend/tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create project structure (backend, frontend folders)
- [x] T002 Initialize .NET 8 Web API project in `backend/`
- [x] T003 Initialize Vue 3 + Vuetify project in `frontend/`
- [x] T004 [P] Create `docker-compose.yml` with Minio service
- [x] T005 [P] Configure backend linting/formatting (EditorConfig)
- [x] T006 [P] Configure frontend linting/formatting (ESLint/Prettier)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T007 [P] Setup Backend DI Container and Swagger
- [x] T008 [P] Setup Frontend Pinia Store and Axios Client
- [x] T009 [P] Create Backend Models (DTOs) for Login and VM Info
- [x] T010 [P] Create Frontend Types/Interfaces for VM and User
- [x] T011 Implement `MinioService` skeleton (Connect, Bucket Check)
- [x] T012 Implement `vSphereService` skeleton (Connect, Session Management)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - vCenter 登入與 VM 列表 (Priority: P1) 🎯 MVP

**Goal**: 使用者能登入 vCenter 並看到 VM 列表

**Independent Test**: 啟動 App，輸入憑證，驗證能看到 VM 清單

### Implementation for User Story 1

- [x] T013 [US1] Implement Backend `POST /api/auth/login` (Connect to vCenter)
- [x] T014 [US1] Implement Backend `GET /api/vms` (List VMs from vCenter)
- [x] T015 [US1] Create Frontend Login View (`views/Login.vue`)
- [x] T016 [US1] Create Frontend VM List Component (`components/VmList.vue`)
- [x] T017 [US1] Integrate Frontend Auth Store with Login API
- [x] T018 [US1] Integrate Frontend VM List with VM API
- [x] T019 [US1] Add Error Handling for Login Failures (UI Feedback)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - 匯出 VM 並上傳至 Minio (Priority: P1)

**Goal**: 使用者能選擇 VM 並匯出至 Minio

**Independent Test**: 點擊匯出，驗證 Minio Bucket 中出現檔案

### Implementation for User Story 2

- [x] T020 [US2] Implement Backend `POST /api/export/{vmId}` (Trigger Export)
- [x] T021 [US2] Implement `vSphereService.ExportVm` logic (OVF Manager/Export)
- [x] T022 [US2] Implement `MinioService.UploadStream` logic
- [x] T023 [US2] Create Backend Task Management (In-Memory Task State)
- [x] T024 [US2] Add "Export" Button to Frontend VM List
- [x] T025 [US2] Integrate Frontend with Export API

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 4 - 即時進度顯示 (Priority: P2)

**Goal**: 匯出過程中顯示進度

**Independent Test**: 匯出時觀察 UI 進度條變化

### Implementation for User Story 4

- [x] T026 [US4] Implement Backend `GET /api/tasks/{taskId}` (Get Progress)
- [x] T027 [US4] Update Backend Export Logic to update Task Progress
- [x] T028 [US4] Create Frontend Progress Component (`components/ExportProgress.vue`)
- [x] T029 [US4] Implement Frontend Polling logic for Task Status

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: User Story 3 - 取得下載連結 (Priority: P1)

**Goal**: 匯出完成後提供下載連結

**Independent Test**: 點擊下載連結，驗證檔案開始下載

### Implementation for User Story 3

- [x] T030 [US3] Implement Backend `MinioService.GetPresignedUrl`
- [x] T031 [US3] Update Backend Task Status to include Download URL upon completion
- [x] T032 [US3] Update Frontend to show Download Button when Task Complete

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T033 [P] Add Dockerfile for Backend
- [x] T034 [P] Add Dockerfile for Frontend
- [x] T035 [P] Update README.md with setup instructions
- [x] T036 [P] Perform End-to-End Testing (Manual)
- [x] T037 [US2] Integrate OVF Tool for real VM export (Backend)

---

## Phase 8: Refinement & Polish (Post-MVP)

**Purpose**: Enhancements for stability, deployment, and branding

- [x] T038 [Infra] Split Docker Compose into Build and Runtime configurations
- [x] T039 [Frontend] Implement Nginx configuration for SPA routing (Fix 404 on reload)
- [x] T040 [Frontend] Implement Session Persistence (Pinia + LocalStorage)
- [x] T041 [UI] Add Branding (Logo and Favicon support)
- [x] T042 [Docs] Update Documentation (README, TASKS, PLAN)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup
- **User Stories (Phase 3-6)**: Depend on Foundational
  - US1 (Login/List) is prerequisite for US2 (Export)
  - US2 (Export) is prerequisite for US4 (Progress) and US3 (Download)

### Within Each User Story

- Backend API before Frontend Integration
- Core Logic before UI Components

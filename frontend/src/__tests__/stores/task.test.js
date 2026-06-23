import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

vi.mock('../../services/exportService', () => ({
  triggerExport: vi.fn(),
  fetchTaskStatus: vi.fn()
}))

import { useTaskStore } from '../../stores/task'
import { triggerExport, fetchTaskStatus } from '../../services/exportService'

describe('useTaskStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('should have default state', () => {
    const store = useTaskStore()

    expect(store.tasks).toEqual({})
    expect(store.error).toBe('')
    expect(store.loading).toBe(false)
  })

  describe('startExport', () => {
    it('should add task and return taskId on success', async () => {
      triggerExport.mockResolvedValue({ taskId: 'task-123' })

      const store = useTaskStore()
      const result = await store.startExport('session', 'vm-1', 'MyVM')

      expect(result).toBe('task-123')
      expect(store.tasks['task-123']).toMatchObject({
        id: 'task-123',
        vmId: 'vm-1',
        vmName: 'MyVM',
        status: 'Running',
        progress: 0
      })
      expect(store.loading).toBe(false)
    })

    it('should set error and return null on failure', async () => {
      triggerExport.mockRejectedValue({ response: { data: '匯出啟動失敗' } })

      const store = useTaskStore()
      const result = await store.startExport('session', 'vm-1', 'MyVM')

      expect(result).toBeNull()
      expect(store.error).toBe('匯出啟動失敗')
      expect(store.loading).toBe(false)
    })

    it('should use fallback error message when response has no data', async () => {
      triggerExport.mockRejectedValue(new Error('Network Error'))

      const store = useTaskStore()
      const result = await store.startExport('session', 'vm-1', 'MyVM')

      expect(result).toBeNull()
      expect(store.error).toBe('匯出啟動失敗')
    })

    it('should set loading to true during request and false after', async () => {
      let resolvePromise
      triggerExport.mockReturnValue(new Promise((resolve) => { resolvePromise = resolve }))

      const store = useTaskStore()
      const exportPromise = store.startExport('session', 'vm-1', 'MyVM')

      expect(store.loading).toBe(true)

      resolvePromise({ taskId: 'task-999' })
      await exportPromise

      expect(store.loading).toBe(false)
    })
  })

  describe('refreshTask', () => {
    it('should update task state and return result on success', async () => {
      const taskData = { id: 'task-1', status: 'Completed', progress: 100, downloadUrl: 'https://example.com/file.ova' }
      fetchTaskStatus.mockResolvedValue(taskData)

      const store = useTaskStore()
      const result = await store.refreshTask('session', 'task-1')

      expect(result).toEqual(taskData)
      expect(store.tasks['task-1']).toEqual(taskData)
    })

    it('should set error and return null on failure', async () => {
      fetchTaskStatus.mockRejectedValue({ response: { data: '查詢任務狀態失敗' } })

      const store = useTaskStore()
      const result = await store.refreshTask('session', 'task-1')

      expect(result).toBeNull()
      expect(store.error).toBe('查詢任務狀態失敗')
    })

    it('should use fallback error message when no response data', async () => {
      fetchTaskStatus.mockRejectedValue(new Error('Timeout'))

      const store = useTaskStore()
      const result = await store.refreshTask('session', 'task-1')

      expect(result).toBeNull()
      expect(store.error).toBe('查詢任務狀態失敗')
    })
  })
})

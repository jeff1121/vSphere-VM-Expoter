import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet, mockPost } = vi.hoisted(() => ({
  mockGet: vi.fn(),
  mockPost: vi.fn()
}))

vi.mock('../../services/apiClient', () => ({
  default: { get: mockGet, post: mockPost }
}))

import { triggerExport, fetchTaskStatus } from '../../services/exportService'

describe('exportService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('triggerExport', () => {
    it('should call POST /api/export/:vmId with session header and vmName param', async () => {
      const mockData = { taskId: 'task-abc-123' }
      mockPost.mockResolvedValue({ data: mockData })

      const result = await triggerExport('my-session', 'vm-1', 'MyVM')

      expect(mockPost).toHaveBeenCalledWith(
        '/api/export/vm-1',
        {},
        {
          headers: { 'X-Session-Id': 'my-session' },
          params: { vmName: 'MyVM' }
        }
      )
      expect(result).toEqual(mockData)
    })

    it('should propagate errors from apiClient', async () => {
      mockPost.mockRejectedValue(new Error('Export failed'))

      await expect(triggerExport('session', 'vm-1', 'VM')).rejects.toThrow('Export failed')
    })
  })

  describe('fetchTaskStatus', () => {
    it('should call GET /api/tasks/:taskId with session header', async () => {
      const mockTask = { id: 'task-1', status: 'Completed', progress: 100 }
      mockGet.mockResolvedValue({ data: mockTask })

      const result = await fetchTaskStatus('my-session', 'task-1')

      expect(mockGet).toHaveBeenCalledWith('/api/tasks/task-1', {
        headers: { 'X-Session-Id': 'my-session' }
      })
      expect(result).toEqual(mockTask)
    })

    it('should propagate errors from apiClient', async () => {
      mockGet.mockRejectedValue(new Error('Task not found'))

      await expect(fetchTaskStatus('session', 'task-1')).rejects.toThrow('Task not found')
    })
  })
})


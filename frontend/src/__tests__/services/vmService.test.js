import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet, mockPost } = vi.hoisted(() => ({
  mockGet: vi.fn(),
  mockPost: vi.fn()
}))

vi.mock('../../services/apiClient', () => ({
  default: { get: mockGet, post: mockPost }
}))

import { fetchVms, powerOffVm } from '../../services/vmService'

describe('vmService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('fetchVms', () => {
    it('should call GET /api/vms with X-Session-Id header', async () => {
      const mockVms = [{ id: 'vm-1', name: 'Test VM', powerState: 'POWERED_ON' }]
      mockGet.mockResolvedValue({ data: mockVms })

      const result = await fetchVms('my-session-id')

      expect(mockGet).toHaveBeenCalledWith('/api/vms', {
        headers: { 'X-Session-Id': 'my-session-id' }
      })
      expect(result).toEqual(mockVms)
    })

    it('should propagate errors from apiClient', async () => {
      const error = new Error('Network Error')
      mockGet.mockRejectedValue(error)

      await expect(fetchVms('session')).rejects.toThrow('Network Error')
    })
  })

  describe('powerOffVm', () => {
    it('should call POST /api/vms/:vmId/power/off with X-Session-Id header', async () => {
      const mockResponse = { message: '已送出關機指令' }
      mockPost.mockResolvedValue({ data: mockResponse })

      const result = await powerOffVm('my-session-id', 'vm-1')

      expect(mockPost).toHaveBeenCalledWith(
        '/api/vms/vm-1/power/off',
        {},
        { headers: { 'X-Session-Id': 'my-session-id' } }
      )
      expect(result).toEqual(mockResponse)
    })

    it('should propagate errors from apiClient', async () => {
      const error = new Error('Unauthorized')
      mockPost.mockRejectedValue(error)

      await expect(powerOffVm('session', 'vm-1')).rejects.toThrow('Unauthorized')
    })
  })
})


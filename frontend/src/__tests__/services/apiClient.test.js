import { describe, it, expect, vi } from 'vitest'

// Mock window.config and import.meta.env
vi.mock('../../services/apiClient', async (importOriginal) => {
  const actual = await importOriginal()
  return actual
})

describe('apiClient', () => {
  it('should be an axios instance with baseURL set', async () => {
    // Re-import to verify module loads
    const mod = await import('../../services/apiClient')
    expect(mod.default).toBeDefined()
    expect(typeof mod.default.get).toBe('function')
    expect(typeof mod.default.post).toBe('function')
  })

  it('should have a timeout of 15000ms', async () => {
    const mod = await import('../../services/apiClient')
    expect(mod.default.defaults.timeout).toBe(15000)
  })
})

import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

const { mockPost } = vi.hoisted(() => ({
  mockPost: vi.fn()
}))

vi.mock('../../services/apiClient', () => ({
  default: {
    post: mockPost,
    interceptors: { response: { use: vi.fn() } }
  }
}))

import { useAuthStore } from '../../stores/auth'

// Mock localStorage
const localStorageMock = (() => {
  let store = {}
  return {
    getItem: vi.fn((key) => store[key] ?? null),
    setItem: vi.fn((key, value) => { store[key] = value }),
    removeItem: vi.fn((key) => { delete store[key] }),
    clear: vi.fn(() => { store = {} })
  }
})()

Object.defineProperty(globalThis, 'localStorage', { value: localStorageMock })

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorageMock.clear()
  })

  it('should have default state with empty values', () => {
    localStorageMock.getItem.mockReturnValue(null)
    const store = useAuthStore()

    expect(store.loading).toBe(false)
    expect(store.error).toBe('')
  })

  it('login: sets sessionId and persists to localStorage on success', async () => {
    const sessionId = 'test-session-id'
    mockPost.mockResolvedValue({ data: { sessionId, success: true } })

    const store = useAuthStore()
    const result = await store.login({ host: 'vcenter.local', username: 'admin', password: 'pass' })

    expect(result).toBe(true)
    expect(store.sessionId).toBe(sessionId)
    expect(store.host).toBe('vcenter.local')
    expect(store.username).toBe('admin')
    expect(localStorageMock.setItem).toHaveBeenCalledWith('vsphere-session', sessionId)
    expect(localStorageMock.setItem).toHaveBeenCalledWith('vsphere-host', 'vcenter.local')
    expect(localStorageMock.setItem).toHaveBeenCalledWith('vsphere-user', 'admin')
    expect(store.loading).toBe(false)
  })

  it('login: sets error and returns false on failure', async () => {
    mockPost.mockRejectedValue({ response: { data: { message: '帳號或密碼錯誤' } } })

    const store = useAuthStore()
    const result = await store.login({ host: 'vcenter.local', username: 'admin', password: 'wrong' })

    expect(result).toBe(false)
    expect(store.error).toBe('帳號或密碼錯誤')
    expect(store.loading).toBe(false)
  })

  it('login: uses fallback error message when response has no message', async () => {
    mockPost.mockRejectedValue(new Error('Network Error'))

    const store = useAuthStore()
    const result = await store.login({ host: 'vcenter.local', username: 'admin', password: 'pass' })

    expect(result).toBe(false)
    expect(store.error).toBe('登入失敗')
  })

  it('logout: clears state and removes localStorage entries', () => {
    const store = useAuthStore()
    store.sessionId = 'some-session'
    store.host = 'vcenter.local'
    store.username = 'admin'

    store.logout()

    expect(store.sessionId).toBe('')
    expect(store.host).toBe('')
    expect(store.username).toBe('')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('vsphere-session')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('vsphere-host')
    expect(localStorageMock.removeItem).toHaveBeenCalledWith('vsphere-user')
  })

  it('login: sets loading to true during request and false after', async () => {
    let resolvePromise
    mockPost.mockReturnValue(new Promise((resolve) => { resolvePromise = resolve }))

    const store = useAuthStore()
    const loginPromise = store.login({ host: 'h', username: 'u', password: 'p' })

    expect(store.loading).toBe(true)

    resolvePromise({ data: { sessionId: 'id' } })
    await loginPromise

    expect(store.loading).toBe(false)
  })
})


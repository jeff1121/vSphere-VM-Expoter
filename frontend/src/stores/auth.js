import { defineStore } from 'pinia'
import apiClient from '../services/apiClient'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    sessionId: localStorage.getItem('vsphere-session') || '',
    host: localStorage.getItem('vsphere-host') || '',
    username: localStorage.getItem('vsphere-user') || '',
    loading: false,
    error: '',
  }),
  actions: {
    async login({ host, username, password }) {
      this.loading = true
      this.error = ''
      try {
        const response = await apiClient.post('/api/auth/login', { host, username, password })
        this.sessionId = response.data.sessionId
        this.host = host
        this.username = username
        
        // Persist to localStorage
        localStorage.setItem('vsphere-session', this.sessionId)
        localStorage.setItem('vsphere-host', this.host)
        localStorage.setItem('vsphere-user', this.username)
        
        return true
      } catch (err) {
        this.error = err?.response?.data?.message || '登入失敗'
        return false
      } finally {
        this.loading = false
      }
    },
    logout() {
      this.sessionId = ''
      this.host = ''
      this.username = ''
      
      // Clear localStorage
      localStorage.removeItem('vsphere-session')
      localStorage.removeItem('vsphere-host')
      localStorage.removeItem('vsphere-user')
    },
  },
})

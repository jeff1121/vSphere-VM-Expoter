import { defineStore } from 'pinia'
import { triggerExport, fetchTaskStatus } from '../services/exportService'

export const useTaskStore = defineStore('tasks', {
  state: () => ({
    tasks: {},
    error: '',
    loading: false,
  }),
  actions: {
    async startExport(sessionId, vmId, vmName) {
      this.loading = true
      this.error = ''
      try {
        const { taskId } = await triggerExport(sessionId, vmId, vmName)
        this.tasks[taskId] = { id: taskId, vmId, vmName, status: 'Running', progress: 10 }
        return taskId
      } catch (err) {
        this.error = err?.response?.data || '匯出啟動失敗'
        return null
      } finally {
        this.loading = false
      }
    },
    async refreshTask(taskId) {
      try {
        const result = await fetchTaskStatus(taskId)
        this.tasks[taskId] = result
        return result
      } catch (err) {
        this.error = err?.response?.data || '查詢任務狀態失敗'
        return null
      }
    },
  },
})

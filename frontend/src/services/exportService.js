import apiClient from './apiClient'

export const triggerExport = async (sessionId, vmId, vmName) => {
  const response = await apiClient.post(
    `/api/export/${vmId}`,
    {},
    {
      headers: { 'X-Session-Id': sessionId },
      params: { vmName }
    }
  )
  return response.data
}

export const fetchTaskStatus = async (sessionId, taskId) => {
  const response = await apiClient.get(`/api/tasks/${taskId}`, {
    headers: { 'X-Session-Id': sessionId },
  })
  return response.data
}

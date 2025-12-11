import apiClient from './apiClient'

export const fetchVms = async (sessionId) => {
  const response = await apiClient.get('/api/vms', {
    headers: { 'X-Session-Id': sessionId },
  })
  return response.data
}

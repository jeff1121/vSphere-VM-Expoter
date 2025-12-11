import apiClient from './apiClient'

export const fetchVms = async (sessionId) => {
  const response = await apiClient.get('/api/vms', {
    headers: { 'X-Session-Id': sessionId },
  })
  return response.data
}

export const powerOffVm = async (sessionId, vmId) => {
  const response = await apiClient.post(
    `/api/vms/${vmId}/power/off`,
    {},
    {
      headers: { 'X-Session-Id': sessionId },
    }
  )
  return response.data
}

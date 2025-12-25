import axios from 'axios'

const baseURL = window.config?.VITE_API_BASE_URL || import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'

const apiClient = axios.create({
  baseURL,
  timeout: 15000,
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Clear local storage and redirect to login
      localStorage.removeItem('vsphere-session')
      window.location.href = '/'
    }
    return Promise.reject(error)
  }
)

export default apiClient

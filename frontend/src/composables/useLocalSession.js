import { watch } from 'vue'
import { useAuthStore } from '../stores/auth'

const KEY = 'vsphere-session'

export const useLocalSession = () => {
  const auth = useAuthStore()

  const stored = localStorage.getItem(KEY)
  if (stored) {
    try {
      const parsed = JSON.parse(stored)
      auth.sessionId = parsed.sessionId || ''
      auth.host = parsed.host || ''
      auth.username = parsed.username || ''
    } catch {
      // ignore parsing errors
    }
  }

  watch(
    () => [auth.sessionId, auth.host, auth.username],
    ([sessionId, host, username]) => {
      const payload = { sessionId, host, username }
      localStorage.setItem(KEY, JSON.stringify(payload))
    },
    { deep: true }
  )

  return { auth }
}

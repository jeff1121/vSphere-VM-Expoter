<script setup>
import { onMounted, onBeforeUnmount, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useTaskStore } from '../stores/task'
import { fetchVms, powerOffVm } from '../services/vmService'

const authStore = useAuthStore()
const taskStore = useTaskStore()
const router = useRouter()

const loading = ref(false)
const vms = ref([])
const error = ref('')
const exportingId = ref('')
const poweringOffId = ref('')
const actionError = ref('')
const actionMessage = ref('')
const search = ref('')
let pollHandle = null

const headers = [
  { title: 'ID', key: 'id' },
  { title: '名稱', key: 'name' },
  { title: '電源狀態', key: 'powerState' },
  { title: '配置容量 (bytes)', key: 'provisionedBytes' },
  { title: '動作', key: 'actions', sortable: false },
]

const loadVms = async () => {
  if (!authStore.sessionId) {
    router.push('/')
    return
  }

  loading.value = true
  error.value = ''
  actionError.value = ''
  try {
    vms.value = await fetchVms(authStore.sessionId)
  } catch (err) {
    error.value = err?.response?.data || '無法取得 VM 列表'
  } finally {
    loading.value = false
  }
}

const isPoweredOn = (state) => {
  const s = (state || '').toString().toUpperCase()
  return s === 'POWERED_ON' || s === 'POWEREDON'
}

const statusLabels = {
  0: 'Pending',
  1: 'Running',
  2: 'Completed',
  3: 'Failed',
}

const formatTaskStatus = (status) => {
  if (typeof status === 'number') {
    return statusLabels[status] ?? status
  }
  return status || '-'
}

const isTerminalStatus = (status) => {
  if (typeof status === 'number') {
    return status === 2 || status === 3
  }
  return status === 'Completed' || status === 'Failed'
}

const startExport = async (vm) => {
  if (!authStore.sessionId) return
  exportingId.value = vm.id
  const taskId = await taskStore.startExport(authStore.sessionId, vm.id, vm.name)
  exportingId.value = ''
  if (taskId) {
    await taskStore.refreshTask(authStore.sessionId, taskId)
    beginPolling()
  }
}

const stopPolling = () => {
  if (!pollHandle) return
  clearInterval(pollHandle)
  pollHandle = null
}

const refreshActiveTasks = async () => {
  const activeTaskIds = Object.values(taskStore.tasks)
    .filter((task) => !isTerminalStatus(task.status))
    .map((task) => task.id)
    .filter(Boolean)

  if (!activeTaskIds.length) {
    return false
  }

  await Promise.all(
    activeTaskIds.map((taskId) => taskStore.refreshTask(authStore.sessionId, taskId))
  )
  return true
}

const beginPolling = () => {
  if (pollHandle) return
  pollHandle = setInterval(async () => {
    if (!authStore.sessionId) {
      stopPolling()
      return
    }
    const hasActive = await refreshActiveTasks()
    if (!hasActive) {
      stopPolling()
    }
  }, 1500)
}

const powerOff = async (vmId) => {
  if (!authStore.sessionId) return
  poweringOffId.value = vmId
  actionError.value = ''
  actionMessage.value = ''
  try {
    const result = await powerOffVm(authStore.sessionId, vmId)
    actionMessage.value = result?.message || '已送出關機指令'
    setTimeout(loadVms, 1500)
  } catch (err) {
    actionError.value = err?.response?.data || '關機失敗'
  } finally {
    poweringOffId.value = ''
  }
}

onMounted(async () => {
  await loadVms()
  if (authStore.sessionId && Object.keys(taskStore.tasks).length) {
    beginPolling()
  }
})

onBeforeUnmount(() => {
  stopPolling()
})
</script>

<template>
  <v-container class="py-10">
    <v-row>
      <v-col cols="12">
        <v-card elevation="2">
          <v-card-title class="d-flex align-center justify-space-between">
            <div>
              <div class="text-h6 font-weight-bold">VM 列表</div>
              <div class="text-caption text-medium-emphasis">
                Host: {{ authStore.host || '未登入' }}｜User: {{ authStore.username || '-' }}
              </div>
            </div>
            <v-btn variant="text" color="primary" @click="loadVms" :loading="loading">重新整理</v-btn>
          </v-card-title>
          <v-divider />
          <v-card-text>
            <v-alert v-if="error" type="error" class="mb-4" density="comfortable">
              {{ error }}
            </v-alert>
            <v-alert v-if="actionError" type="error" class="mb-4" density="comfortable">
              {{ actionError }}
            </v-alert>
            <v-alert v-if="actionMessage" type="success" class="mb-4" density="comfortable">
              {{ actionMessage }}
            </v-alert>
            
            <v-text-field
              v-model="search"
              prepend-inner-icon="mdi-magnify"
              label="搜尋 VM"
              single-line
              hide-details
              density="compact"
              class="mb-4"
              variant="outlined"
            ></v-text-field>

            <v-data-table
              :headers="headers"
              :items="vms"
              :search="search"
              :loading="loading"
              items-per-page="10"
              class="elevation-1"
            >
              <template v-slot:item.actions="{ item }">
                <div class="d-flex ga-2 flex-wrap">
                  <v-btn size="small" color="primary" :loading="exportingId === item.id || taskStore.loading" @click="startExport(item)">
                    匯出
                  </v-btn>
                  <v-btn
                    size="small"
                    color="error"
                    variant="tonal"
                    :disabled="!isPoweredOn(item.powerState)"
                    :loading="poweringOffId === item.id"
                    @click="powerOff(item.id)"
                  >
                    關機
                  </v-btn>
                </div>
              </template>
              <template v-slot:no-data>
                <div class="text-medium-emphasis text-center py-6">
                  尚無資料
                </div>
              </template>
            </v-data-table>

            <div v-if="Object.keys(taskStore.tasks).length" class="mt-6">
              <div class="text-subtitle-2 mb-2">任務狀態</div>
              <v-table density="compact">
                <thead>
                  <tr>
                    <th class="text-left">Task Id</th>
                    <th class="text-left">VM</th>
                    <th class="text-left">狀態</th>
                    <th class="text-left">進度</th>
                    <th class="text-left">錯誤</th>
                    <th class="text-left">下載連結</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="task in Object.values(taskStore.tasks)" :key="task.id">
                    <td>{{ task.id }}</td>
                    <td>{{ task.vmId }}</td>
                    <td>{{ formatTaskStatus(task.status) }}</td>
                    <td>{{ task.progress ?? '-' }}%</td>
                    <td class="text-error" style="max-width: 240px; white-space: normal;">
                      <span v-if="task.error">{{ task.error }}</span>
                      <span v-else>-</span>
                    </td>
                    <td>
                      <a v-if="task.downloadUrl" :href="task.downloadUrl" target="_blank">下載</a>
                      <span v-else>-</span>
                    </td>
                  </tr>
                </tbody>
              </v-table>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

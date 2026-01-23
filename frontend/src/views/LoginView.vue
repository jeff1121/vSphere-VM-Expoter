<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import logo from '@/assets/logo.jpg'

const authStore = useAuthStore()
const router = useRouter()

const host = ref('')
const username = ref('')
const password = ref('')

const onSubmit = async () => {
  const success = await authStore.login({ host: host.value, username: username.value, password: password.value })
  if (success) {
    router.push('/dashboard')
  }
  password.value = ''
}
</script>

<template>
  <v-container class="page-shell auth-shell">
    <v-row class="auth-grid" align="center" justify="center">
      <v-col cols="12" md="6" class="auth-media">
        <div class="logo-wrap">
          <v-img
            :src="logo"
            alt="vSphere VM Exporter"
            max-width="520"
            class="login-logo"
            contain
          ></v-img>
        </div>
        <div class="auth-copy">
          <div class="text-h5 font-weight-bold">vSphere VM Exporter</div>
          <div class="text-body-2 text-medium-emphasis">
            安全登入 vCenter，快速管理與匯出 VM。
          </div>
        </div>
      </v-col>
      <v-col cols="12" md="6">
        <v-card elevation="0" class="glass-card auth-card pa-8">
          <v-card-title class="text-h5 font-weight-bold text-center">登入 vCenter</v-card-title>
          <v-card-subtitle class="text-center">請輸入連線資訊</v-card-subtitle>
          <v-card-text class="pt-4">
            <v-form @submit.prevent="onSubmit" class="form-stack">
              <v-text-field v-model="host" label="vCenter Host" required clearable class="glass-input" />
              <v-text-field v-model="username" label="Username" required clearable class="glass-input" />
              <v-text-field v-model="password" label="Password" type="password" required clearable class="glass-input" />
              <v-alert v-if="authStore.error" type="error" class="mb-4" density="comfortable">
                {{ authStore.error }}
              </v-alert>
              <v-btn :loading="authStore.loading" type="submit" class="btn-primary" block size="large">登入</v-btn>
            </v-form>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

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
  <v-container class="py-10" max-width="600">
    <div class="d-flex justify-center mb-8">
      <v-img :src="logo" max-width="600" contain></v-img>
    </div>
    <v-card elevation="2" class="pa-8">
      <v-card-title class="text-h5 font-weight-bold text-center">登入 vCenter</v-card-title>
      <v-card-text class="pt-4">
        <v-form @submit.prevent="onSubmit">
          <v-text-field v-model="host" label="vCenter Host" required clearable />
          <v-text-field v-model="username" label="Username" required clearable />
          <v-text-field v-model="password" label="Password" type="password" required clearable />
          <v-alert v-if="authStore.error" type="error" class="mb-4" density="comfortable">
            {{ authStore.error }}
          </v-alert>
          <v-btn :loading="authStore.loading" type="submit" color="primary" block>登入</v-btn>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import 'vuetify/styles'
import './style.css'
import App from './App.vue'
import router from './router'
import { useLocalSession } from './composables/useLocalSession'

const vuetify = createVuetify({
  components,
  directives,
})

const pinia = createPinia()

const app = createApp(App).use(pinia).use(router).use(vuetify)

// Restore persisted session before mounting
useLocalSession()

app.mount('#app')

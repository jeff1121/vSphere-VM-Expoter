import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import 'vuetify/styles'
import './style.css'
import App from './App.vue'
import router from './router'

const vuetify = createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'vSphereLight',
    themes: {
      vSphereLight: {
        dark: false,
        colors: {
          background: '#EAF0F7',
          surface: '#F8FAFC',
          primary: '#3B82F6',
          secondary: '#60A5FA',
          accent: '#F97316',
          error: '#EF4444',
          warning: '#F59E0B',
          info: '#38BDF8',
          success: '#10B981',
        },
        variables: {
          'border-color': '#E2E8F0',
          'border-opacity': 0.6,
        },
      },
    },
  },
  defaults: {
    VCard: {
      rounded: 'xl',
    },
    VTextField: {
      variant: 'outlined',
      density: 'comfortable',
      color: 'primary',
    },
    VAlert: {
      variant: 'tonal',
      density: 'comfortable',
    },
    VDataTable: {
      density: 'comfortable',
    },
  },
})

const pinia = createPinia()

const app = createApp(App).use(pinia).use(router).use(vuetify)

app.mount('#app')

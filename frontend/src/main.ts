import './assets/index.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { initializeApiClient } from './api/api'
import { MotionPlugin } from '@vueuse/motion'

const app = createApp(App)

app.use(createPinia())
app.use(MotionPlugin)
app.use(router)

initializeApiClient()

app.mount('#app')

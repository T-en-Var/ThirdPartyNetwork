import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueDevTools from 'vite-plugin-vue-devtools';
import autoprefixer from 'autoprefixer';

// Set by docker-compose (infra/setup.ps1 README) when running behind the
// local HTTPS proxy, so `npm run dev` outside Docker is unaffected.
const runningBehindDockerProxy = process.env.DOCKER_DEV === 'true';

// https://vite.dev/config/
export default defineConfig({
  server: {
    host: true,
    allowedHosts: ['mytpn.tijmensimons.nl', 'tpn.test', 'localhost'],
    ...(runningBehindDockerProxy && {
      // Bind-mounted source on Windows/Docker Desktop doesn't reliably emit
      // native file-change events, so fall back to polling for hot reload.
      watch: { usePolling: true },
      hmr: {
        protocol: 'wss',
        host: 'tpn.test',
        clientPort: 443,
      },
    }),
  },
  plugins: [vue(), vueDevTools()],
  css: {
    postcss: {
      plugins: [autoprefixer()],
    },
    preprocessorOptions: {
      scss: {
        additionalData: `@use "@/assets/styles/variables" as *;`,
      },
    },
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
});

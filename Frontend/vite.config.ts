import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        // localhost -> ::1 (IPv6) olabiliyor; Kestrel cogu kurulumda sadece IPv4 dinler → ECONNREFUSED
        target: 'http://127.0.0.1:5198',
        changeOrigin: true,
      },
      // API `wwwroot/images/...` → VITE_API_BASE_URL yokken göreli `/images/...` çalışsın
      '/images': {
        target: 'http://127.0.0.1:5198',
        changeOrigin: true,
      },
    },
  },
  preview: {
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:5198',
        changeOrigin: true,
      },
      '/images': {
        target: 'http://127.0.0.1:5198',
        changeOrigin: true,
      },
    },
  },
})

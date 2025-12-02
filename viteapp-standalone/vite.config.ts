import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: process.env.PYTHONAPP_STANDALONE_HTTP || 'http://localhost:8000',
        changeOrigin: true,
      }
    }
  }
})

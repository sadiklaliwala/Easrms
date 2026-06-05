import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      "/api": {
        target: "http://localhost:5118",
        changeOrigin: true,
        secure: false,
      },
      "/hubs": {
        target: "http://localhost:5118",
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
  build: {
    outDir: 'dist',
    rollupOptions: {
      output: {
        manualChunks: {
          // Core React runtime — always cached first
          'vendor-react': ['react', 'react-dom', 'react-router-dom'],

          // Redux + RTK Query
          'vendor-redux': ['@reduxjs/toolkit', 'react-redux'],

          // MUI core components
          'vendor-mui-core': ['@mui/material', '@mui/system', '@emotion/react', '@emotion/styled'],

          // MUI icons (large — split away from core)
          'vendor-mui-icons': ['@mui/icons-material'],

          // Form handling + validation
          'vendor-forms': ['react-hook-form', '@hookform/resolvers', 'joi'],

          // Charts (recharts is heavy)
          'vendor-charts': ['recharts'],

          // SignalR (large, only needed post-login)
          'vendor-signalr': ['@microsoft/signalr'],

          // Misc utilities
          'vendor-misc': ['react-hot-toast'],
        },
      },
    },
  },
});

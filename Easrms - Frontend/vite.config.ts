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
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (id.includes('@mui')) {
              // Group all MUI packages together to prevent circular dependency errors like "Cannot set properties of undefined"
              return 'vendor-mui';
            }
            if (id.includes('react') || id.includes('@remix-run') || id.includes('router')) {
              return 'vendor-react';
            }
            if (id.includes('redux')) {
              return 'vendor-redux';
            }
            if (id.includes('joi')) {
              return 'vendor-joi';
            }
            if (id.includes('@microsoft/signalr')) {
              return 'vendor-signalr';
            }
            if (id.includes('recharts') || id.includes('d3')) {
              return 'vendor-charts';
            }
            return 'vendor-core'; // Fallback for other node_modules
          }
        },
      },
    },
  },
});

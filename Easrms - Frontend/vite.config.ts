import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";

// https://vite.dev/config/
export default defineConfig(({ mode }) => ({
  plugins: [react()],
  define: {
    // Only mock process.env in development to avoid breaking production builds
    ...(mode === "development" ? { "process.env": {} } : {}),
  },
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
    outDir: "dist",
  },
}));

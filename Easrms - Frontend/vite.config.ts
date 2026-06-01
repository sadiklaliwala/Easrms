import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import basicSsl from "@vitejs/plugin-basic-ssl";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), basicSsl()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "https://localhost:7252",
        changeOrigin: true,
        secure: false,
      },
      "/hubs": {
        target: "https://localhost:7252",
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
  build: {
    outDir: 'dist',
  },
});

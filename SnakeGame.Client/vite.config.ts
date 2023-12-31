import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

import fs from "fs";
import path from "path";
import { execSync } from "child_process";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 8000,
    strictPort: true,
    https: generateCerts(),
    proxy: {
      "/gamehub": {
        secure: false,
        target: process.env.ASPNETCORE_HTTPS_PORT
          ? `ws://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
          : process.env.ASPNETCORE_URLS
          ? process.env.ASPNETCORE_URLS.split(";")[0]
          : "ws://localhost:3000",
        ws: true,
      },
      "/api/v1": {
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api\/v1/, "/api/v1"),
        // target taken from src/setupProxy.js in ASP.NET React template
        target: process.env.ASPNETCORE_HTTPS_PORT
          ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
          : process.env.ASPNETCORE_URLS
          ? process.env.ASPNETCORE_URLS.split(";")[0]
          : "http://localhost:3000",

        configure: (proxy, _options) => {
          proxy.on("error", (err, _req, _res) => {
            console.log("proxy error", err);
          });
          proxy.on("proxyReq", (proxyReq, req, _res) => {
            console.log("Sending Request to the Target:", req.method, req.url);
          });
          proxy.on("proxyRes", (proxyRes, req, _res) => {
            console.log(
              "Received Response from the Target:",
              proxyRes.statusCode,
              req.url,
            );
          });
        },
      },
    },
  },
});

function generateCerts() {
  const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ""
      ? `${process.env.APPDATA}/ASP.NET/https`
      : `${process.env.HOME}/.aspnet/https`;
  const certificateArg = process.argv
    .map((arg) => arg.match(/--name=(?<value>.+)/i))
    .filter(Boolean)[0];
  const certificateName = certificateArg
    ? certificateArg.groups.value
    : process.env.npm_package_name;

  if (!certificateName) {
    console.error(
      "Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.",
    );
    process.exit(-1);
  }

  const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
  const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

  if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    const outp = execSync(
      "dotnet " +
        [
          "dev-certs",
          "https",
          "--export-path",
          certFilePath,
          "--format",
          "Pem",
          "--no-password",
        ].join(" "),
    );
    console.log(outp.toString());
  }

  return {
    cert: fs.readFileSync(certFilePath, "utf8"),
    key: fs.readFileSync(keyFilePath, "utf8"),
  };
}

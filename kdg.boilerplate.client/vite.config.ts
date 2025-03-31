import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "kdg.boilerplate.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

// Ensure the certificate directory exists with appropriate permissions
if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true, mode: 0o700 }); // Only owner can read/write/execute
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
    
    // Set appropriate permissions for certificate files
    fs.chmodSync(certFilePath, 0o600); // Only owner can read/write
    fs.chmodSync(keyFilePath, 0o600);  // Only owner can read/write
}

const target = env.ASPNETCORE_HTTPS_PORT ? `https://0.0.0.0:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://0.0.0.0:7027';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '^/api': {
                target,
                secure: false
            }
        },
        port: 5173,
        host: true,

        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        },
        watch: {
            usePolling: true,
        }
    },
    css: {
        preprocessorOptions: {
            scss: {
                api: 'modern'
            }
        }
    }
})

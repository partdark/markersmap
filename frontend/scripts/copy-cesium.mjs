// Копирует статику Cesium (Workers/Assets/ThirdParty/Widgets) в public/cesium,
// чтобы Cesium мог загружать воркеры и ассеты в dev-режиме и при сборке.
import { cpSync, existsSync, mkdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const src = join(root, 'node_modules', 'cesium', 'Build', 'Cesium');
const dest = join(root, 'public', 'cesium');

if (!existsSync(src)) {
  console.error('Cesium build not found. Run `npm install` first.');
  process.exit(1);
}

mkdirSync(dest, { recursive: true });
for (const sub of ['Workers', 'Assets', 'ThirdParty', 'Widgets']) {
  cpSync(join(src, sub), join(dest, sub), { recursive: true });
}
console.log('Cesium assets copied to public/cesium');
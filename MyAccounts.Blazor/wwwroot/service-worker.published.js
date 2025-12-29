// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => onMessage(event));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/ ];
// Exclude service worker files from caching to allow version detection
const offlineAssetsExclude = [ /^service-worker\.js$/, /^service-worker-assets\.js$/ ];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install - Version: ' + self.assetsManifest.version);

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));

    // Force the waiting service worker to become the active service worker immediately
    self.skipWaiting();
}

async function onActivate(event) {
    console.info('Service worker: Activate - Version: ' + self.assetsManifest.version);

    // Delete ALL old caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => {
            console.info('Service worker: Deleting old cache: ' + key);
            return caches.delete(key);
        }));

    // Take control of all clients immediately
    await self.clients.claim();
    
    // Notify all clients about the update
    const clients = await self.clients.matchAll({ type: 'window' });
    clients.forEach(client => {
        client.postMessage({ type: 'SW_UPDATED', version: self.assetsManifest.version });
    });
}

async function onFetch(event) {
    const requestUrl = new URL(event.request.url);
    
    // Never cache the service-worker-assets.js file - always fetch from network
    if (requestUrl.pathname.endsWith('service-worker-assets.js')) {
        return fetch(event.request, { cache: 'no-store' });
    }
    
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache,
        // unless that request is for an offline resource.
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    return cachedResponse || fetch(event.request);
}

function onMessage(event) {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        console.info('Service worker: Received SKIP_WAITING message');
        self.skipWaiting();
    }
}

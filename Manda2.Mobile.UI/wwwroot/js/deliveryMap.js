// PROPÓSITO: Gestión del mapa Azure Maps para DeliveryMapComponent.razor.
//            Reemplaza completamente la implementación anterior con Leaflet.
//
// FUNCIONES EXPORTADAS:
//   initDeliveryMap(elementId, lat, lon, clientId, dotNetRef)
//   setMapCenter(elementId, lat, lon)
//   destroyMap(elementId)
//   tryGetDeviceLocation(timeoutMs)
// ─────────────────────────────────────────────────────────────────────────────

const _mapInstances = {};   // { elementId: { map, marker, datasource } }

// ── Carga dinámica del SDK de Azure Maps ──────────────────────────────────────
function loadAzureMapsSDK() {
    return new Promise((resolve, reject) => {
        if (window.atlas) { resolve(); return; }
        const link = document.createElement('link');
        link.rel = 'stylesheet'; link.href = 'https://atlas.microsoft.com/sdk/javascript/mapcontrol/3/atlas.min.css';
        document.head.appendChild(link);
        const script = document.createElement('script');
        script.src = 'https://atlas.microsoft.com/sdk/javascript/mapcontrol/3/atlas.min.js';
        script.onload = () => resolve(); script.onerror = () => reject(new Error('Error SDK'));
        document.head.appendChild(script);
    });
}

// ── Inicializa el mapa Azure Maps ─────────────────────────────────────────────
export async function initDeliveryMap(elementId, lat, lon, subscriptionKey, dotNetRef) {
    // Validar que lat y lon sean números válidos
    const validLat = (lat !== null && lat !== undefined) ? lat : 14.6349;
    const validLon = (lon !== null && lon !== undefined) ? lon : -90.5069;

    await loadAzureMapsSDK();
    if (_mapInstances[elementId]) { _mapInstances[elementId].map.dispose(); delete _mapInstances[elementId]; }

    // Crear mapa con autenticación por Client ID (AAD Managed Identity)
    const map = new atlas.Map(elementId, {
        center: [lon, lat],
        zoom: 16,
        language: 'es-419',
        authOptions: { authType: 'subscriptionKey', subscriptionKey: subscriptionKey },
        style: 'road',
        disableTelemetry: true
    });

    // Esperar a que el mapa esté listo
    map.events.add('ready', () => {
        // En lugar de SymbolLayer, usamos HtmlMarker para que sea draggable fácilmente
        const marker = new atlas.HtmlMarker({
            draggable: true,
            color: 'Red',
            position: [lon, lat]
        });

        // Evento: Al terminar de arrastrar el pin
        map.events.add('dragend', marker, (e) => {
            const pos = marker.getOptions().position;
            dotNetRef.invokeMethodAsync('OnMapMoved', pos[1], pos[0]);
        });
        
        // Evento: Click en el mapa mueve el pin
        map.events.add('click', (e) => {
            if (!e.position) return;
            marker.setOptions({ position: e.position });
            dotNetRef.invokeMethodAsync('OnMapMoved', e.position[1], e.position[0]);
        });

        map.markers.add(marker);
        _mapInstances[elementId] = { map, marker };
    });
}

// ── Mueve el marcador y centra el mapa ────────────────────────────────────────
export function setMapCenter(elementId, lat, lon) {
    const inst = _mapInstances[elementId];
    if (!inst) return;
    inst.map.setCamera({ center: [lon, lat], zoom: 16, type: 'ease', duration: 500 });
    inst.marker.setOptions({ position: [lon, lat] });
}

// ── Mueve el marcador en el datasource ───────────────────────────────────────
function _moveMarker(elementId, lat, lon) {
    const instance = _mapInstances[elementId];
    if (!instance) return;

    instance.datasource.clear();
    const newPoint = new atlas.data.Feature(new atlas.data.Point([lon, lat]));
    instance.datasource.add(newPoint);
    instance.point = newPoint;
}

// ── Destruye el mapa y libera recursos ───────────────────────────────────────
export function destroyMap(elementId) {
    if (_mapInstances[elementId]) {
        _mapInstances[elementId].map.dispose();
        delete _mapInstances[elementId];
    }
}

// ── Obtiene ubicación del dispositivo vía browser Geolocation API ─────────────
export async function tryGetDeviceLocation(timeoutMs = 8000) {
    return new Promise((resolve) => {
        if (!navigator.geolocation) { resolve(null); return; }
        navigator.geolocation.getCurrentPosition(
            (p) => resolve({ latitude: p.coords.latitude, longitude: p.coords.longitude }),
            () => resolve(null),
            { enableHighAccuracy: true, timeout: timeoutMs }
        );
    });
}

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

        // CSS
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = 'https://atlas.microsoft.com/sdk/javascript/mapcontrol/3/atlas.min.css';
        document.head.appendChild(link);

        // JS
        const script = document.createElement('script');
        script.src = 'https://atlas.microsoft.com/sdk/javascript/mapcontrol/3/atlas.min.js';
        script.onload = () => resolve();
        script.onerror = () => reject(new Error('[DeliveryMap] No se pudo cargar Azure Maps SDK.'));
        document.head.appendChild(script);
    });
}

// ── Inicializa el mapa Azure Maps ─────────────────────────────────────────────
export async function initDeliveryMap(elementId, lat, lon, clientId, dotNetRef) {
    try {
        await loadAzureMapsSDK();
    } catch (e) {
        console.error(e.message);
        return;
    }

    // Destruir instancia previa si existe
    if (_mapInstances[elementId]) {
        try { _mapInstances[elementId].map.dispose(); } catch { }
        delete _mapInstances[elementId];
    }

    // Crear mapa con autenticación por Client ID (AAD Managed Identity)
    const map = new atlas.Map(elementId, {
        center: [lon, lat],   // Azure Maps: [longitude, latitude]
        zoom: 15,
        language: 'es-419',
        view: 'Auto',
        authOptions: {
            authType: 'subscriptionKey',
            subscriptionKey: clientId   // En modo profesional, clientId = subscriptionKey del frontend
        },
        style: 'road'
    });

    // Esperar a que el mapa esté listo
    map.events.add('ready', function () {

        // DataSource para el marcador
        const datasource = new atlas.source.DataSource();
        map.sources.add(datasource);

        // Marcador en posición inicial
        const point = new atlas.data.Feature(new atlas.data.Point([lon, lat]));
        datasource.add(point);

        // Capa de símbolo (marcador visual)
        const symbolLayer = new atlas.layer.SymbolLayer(datasource, null, {
            iconOptions: {
                image: 'pin-red',
                anchor: 'bottom',
                allowOverlap: true
            }
        });
        map.layers.add(symbolLayer);

        // Evento: click en el mapa → mover marcador
        map.events.add('click', function (e) {
            if (!e.position) return;
            const [clickLon, clickLat] = e.position;
            _moveMarker(elementId, clickLat, clickLon);
            dotNetRef.invokeMethodAsync('OnMapMoved', clickLat, clickLon);
        });

        // Guardar instancia
        _mapInstances[elementId] = { map, datasource, point };
    });
}

// ── Mueve el marcador y centra el mapa ────────────────────────────────────────
export function setMapCenter(elementId, lat, lon) {
    const instance = _mapInstances[elementId];
    if (!instance) {
        console.warn(`[DeliveryMap] setMapCenter: no existe instancia para (${elementId})`);
        return;
    }

    instance.map.setCamera({
        center: [lon, lat],
        zoom: 15,
        type: 'ease',
        duration: 500
    });

    _moveMarker(elementId, lat, lon);
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
    const instance = _mapInstances[elementId];
    if (!instance) return;

    try { instance.map.dispose(); } catch (e) {
        console.warn(`[DeliveryMap] Error al destruir mapa (${elementId}):`, e);
    }

    delete _mapInstances[elementId];
}

// ── Obtiene ubicación del dispositivo vía browser Geolocation API ─────────────
export async function tryGetDeviceLocation(timeoutMs = 8000) {
    return new Promise((resolve) => {
        try {
            if (!('geolocation' in navigator)) { resolve(null); return; }

            let resolved = false;
            const finish = (value) => { if (resolved) return; resolved = true; resolve(value); };

            navigator.geolocation.getCurrentPosition(
                (pos) => finish({ latitude: pos.coords.latitude, longitude: pos.coords.longitude }),
                () => finish(null),
                { enableHighAccuracy: true, maximumAge: 0, timeout: timeoutMs }
            );

            setTimeout(() => finish(null), timeoutMs + 300);
        } catch {
            resolve(null);
        }
    });
}

export function updateMapCenter(elementId, lat, lon) {
    const map = _mapInstances[elementId];
    if (!map) return;

    map.setView([lat, lon], 16);

    // Mover el marcador si existe
    map.eachLayer(function (layer) {
        if (layer instanceof L.Marker) {
            layer.setLatLng([lat, lon]);
        }
    });
}
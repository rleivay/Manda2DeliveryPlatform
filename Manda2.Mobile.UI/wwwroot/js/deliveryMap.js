// deliveryMap.js - Azure Maps v3 para MAUI Blazor Hybrid

window.DeliveryMap = {
    init: function (elementId, lat, lon, subscriptionKey, dotNetRef) {
        try {
            // Defensa explícita de tipos
            const nLat = (typeof lat === 'number' && !Number.isNaN(lat)) ? lat : 14.6349;
            const nLon = (typeof lon === 'number' && !Number.isNaN(lon)) ? lon : -90.5069;

            const map = new atlas.Map(elementId, {
                center: [nLon ?? -90.5069, nLat ?? 14.6349],
                zoom: 14,
                minZoom: 1,
                maxZoom: 20,
                pitch: 0,          // Evita null interno en el cálculo de cámara
                bearing: 0,        // Evita null interno en el cálculo de cámara
                language: 'es-419',
                style: 'road',
                view: 'Auto',
                authOptions: {
                    authType: 'subscriptionKey',
                    subscriptionKey: subscriptionKey
                }
            });

            map.events.add('ready', () => {
                try {
                    // NOTA: HtmlMarker en v3 NO soporta 'draggable' oficialmente.
                    // Pasarlo genera warnings internos de propiedades numéricas nulas.
                    const marker = new atlas.HtmlMarker({
                        position: [nLon, nLat],
                        color: 'red'
                    });
                    map.markers.add(marker);

                    // Click en el mapa -> reposicionar marcador
                    map.events.add('click', (e) => {
                        try {
                            if (!e || !e.position) return;

                            const pos = e.position; // [lon, lat]
                            if (!Array.isArray(pos) || pos.length < 2 || pos[0] == null || pos[1] == null) return;

                            marker.setOptions({ position: pos });
                            dotNetRef.invokeMethodAsync('OnMapMoved', pos[1], pos[0]);
                        } catch (err) {
                            console.error('[DeliveryMap] Error en click:', err);
                        }
                    });

                    // Si necesitas drag nativo del marcador en v3, se implementa
                    // manualmente con mousedown/mousemove/mouseup del mapa.
                    // El navegador/WebView puede permitir arrastrar el DOM del HtmlMarker
                    // por defecto, pero no es comportamiento garantizado de la API.
                } catch (err) {
                    console.error('[DeliveryMap] Error en ready:', err);
                }
            });

            window._deliveryMapInstance = map;
        } catch (err) {
            console.error('[DeliveryMap] Error al inicializar mapa:', err);
        }
    },

    setCenter: function (elementId, lat, lon) {
        try {
            const map = window._deliveryMapInstance;
            if (!map) return;
            const nLat = (typeof lat === 'number' && !Number.isNaN(lat)) ? lat : 14.6349;
            const nLon = (typeof lon === 'number' && !Number.isNaN(lon)) ? lon : -90.5069;
            map.setCamera({ center: [nLon, nLat], zoom: 14 });
        } catch (err) {
            console.error('[DeliveryMap] Error en setCenter:', err);
        }
    },

    destroy: function (elementId) {
        try {
            if (window._deliveryMapInstance) {
                window._deliveryMapInstance.dispose();
                window._deliveryMapInstance = null;
            }
            const el = document.getElementById(elementId);
            if (el) el.innerHTML = '';
        } catch (err) {
            console.error('[DeliveryMap] Error al destruir mapa:', err);
        }
    }
};
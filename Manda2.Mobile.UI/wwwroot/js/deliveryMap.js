// deliveryMap.js - Azure Maps v3 para MAUI Blazor Hybrid
// EXPONE: init, setCenter, destroy, tryGetDeviceLocation, getMarkerPosition

(function () {
    'use strict';
    const _instances = {}; // elementId -> { map, marker, dotNetRef }

    function _ensureNumbers(lat, lon) {
        const okLat = (typeof lat === 'number' && !Number.isNaN(lat)) ? lat : 14.6349;
        const okLon = (typeof lon === 'number' && !Number.isNaN(lon)) ? lon : -90.5069;
        return { lat: okLat, lon: okLon };
    }

    function init(elementId, lat, lon, subscriptionKey, dotNetRef) {
        try {
            const el = document.getElementById(elementId);
            if (!el) {
                console.error('[DeliveryMap] Elemento #' + elementId + ' no existe en DOM.');
                return;
            }
            if (el.offsetWidth === 0 || el.offsetHeight === 0) {
                console.warn('[DeliveryMap] Contenedor #' + elementId + ' tiene tamaño 0. Reintentando en 300ms...');
                setTimeout(function () {
                    init(elementId, lat, lon, subscriptionKey, dotNetRef);
                }, 300);
                return;
            }
            if (!window.atlas) {
                console.error('[DeliveryMap] Azure Maps SDK (atlas) no está cargado.');
                return;
            }
            if (!subscriptionKey) {
                console.error('[DeliveryMap] subscriptionKey vacío.');
                return;
            }

            const coords = _ensureNumbers(lat, lon);

            if (_instances[elementId]) {
                try { _instances[elementId].map.dispose(); } catch { }
                delete _instances[elementId];
            }

            const map = new atlas.Map(elementId, {
                center: [coords.lon, coords.lat],
                zoom: 16,
                pitch: 0,
                bearing: 0,
                minZoom: 1,
                maxZoom: 20,
                language: 'es-419',
                style: 'road',
                view: 'Auto',
                authOptions: {
                    authType: 'subscriptionKey',
                    subscriptionKey: String(subscriptionKey).trim()
                }
            });

            map.events.add('ready', function () {
                try {
                    const marker = new atlas.HtmlMarker({
                        position: [coords.lon, coords.lat],
                        color: 'red'
                    });
                    map.markers.add(marker);

                    // Click en el mapa -> mover marcador y notificar .NET
                    map.events.add('click', function (e) {
                        try {
                            if (!e || !e.position) return;
                            const pos = e.position; // [lon, lat]
                            if (!Array.isArray(pos) || pos.length < 2 || pos[0] == null || pos[1] == null) return;

                            marker.setOptions({ position: pos });
                            if (dotNetRef) {
                                dotNetRef.invokeMethodAsync('OnMapMoved', pos[1], pos[0]);
                            }
                        } catch (err) {
                            console.error('[DeliveryMap] click error:', err);
                        }
                    });

                    _instances[elementId] = { map: map, marker: marker, dotNetRef: dotNetRef };
                } catch (err) {
                    console.error('[DeliveryMap] ready error:', err);
                }
            });
        } catch (err) {
            console.error('[DeliveryMap] init error:', err);
        }
    }

    function setCenter(elementId, lat, lon) {
        try {
            const inst = _instances[elementId];
            if (!inst) return;
            const coords = _ensureNumbers(lat, lon);
            inst.map.setCamera({ center: [coords.lon, coords.lat], zoom: 16, type: 'ease', duration: 350 });
            inst.marker.setOptions({ position: [coords.lon, coords.lat] });
        } catch (err) {
            console.error('[DeliveryMap] setCenter error:', err);
        }
    }

    function getMarkerPosition(elementId) {
        try {
            const inst = _instances[elementId];
            if (!inst) return null;
            const pos = inst.marker.getOptions().position;
            if (!Array.isArray(pos) || pos.length < 2) return null;
            return { latitude: pos[1], longitude: pos[0] };
        } catch (err) {
            console.error('[DeliveryMap] getMarkerPosition error:', err);
            return null;
        }
    }

    function destroy(elementId) {
        try {
            const inst = _instances[elementId];
            if (!inst) return;
            try { inst.map.dispose(); } catch { }
            delete _instances[elementId];
        } catch (err) {
            console.error('[DeliveryMap] destroy error:', err);
        }
    }

    function tryGetDeviceLocation(timeoutMs) {
        timeoutMs = timeoutMs || 8000;
        return new Promise(function (resolve) {
            if (!('geolocation' in navigator)) { resolve(null); return; }
            let resolved = false;
            function finish(v) { if (resolved) return; resolved = true; resolve(v); }
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    finish({ latitude: pos.coords.latitude, longitude: pos.coords.longitude });
                },
                function () { finish(null); },
                { enableHighAccuracy: true, maximumAge: 0, timeout: timeoutMs }
            );
            setTimeout(function () { finish(null); }, timeoutMs + 300);
        });
    }

    window.DeliveryMap = {
        init: init,
        setCenter: setCenter,
        getMarkerPosition: getMarkerPosition,
        destroy: destroy,
        tryGetDeviceLocation: tryGetDeviceLocation
    };
})();
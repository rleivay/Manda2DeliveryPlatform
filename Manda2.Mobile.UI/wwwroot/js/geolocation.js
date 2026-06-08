// Función global para obtener GPS del dispositivo desde Blazor
window.getDeviceLocation = () => {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject('Geolocation not supported');
            return;
        }
        navigator.geolocation.getCurrentPosition(
            pos => resolve({
                latitude: pos.coords.latitude,
                longitude: pos.coords.longitude
            }),
            err => reject(err.message),
            { enableHighAccuracy: true, timeout: 8000 }
        );
    });
};
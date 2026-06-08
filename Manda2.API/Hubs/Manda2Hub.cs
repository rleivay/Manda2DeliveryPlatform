// PROPÓSITO: Hub central de SignalR para la plataforma Manda2.
//            Gestiona tres canales en tiempo real:
//            1. GPS del driver → broadcast al cliente y BackOffice.
//            2. Notificaciones de dispatch → oferta nueva al driver.
//            3. Cambios de estado de orden → push al cliente y comercio.
//
// GRUPOS DE CONEXIÓN:
//   "driver_{driverId}"        → conexiones del driver (su app móvil).
//   "group_{orderGroupId}"     → cliente + driver + comercio de ese grupo.
//   "merchant_{merchantId}"    → conexiones del comercio.
//   "backoffice"               → todos los operadores BackOffice.
//
// SEGURIDAD:
//   - [Authorize] en el Hub → solo usuarios con JWT válido conectan.
//   - Cada método valida que el claim del JWT coincida con el actor.
//
// DEPENDENCIAS:
//   - IDriverLocationCache: abstracción de caché GPS (in-memory o Redis).
//   - IApplicationDbContext: para validar existencia de entidades.
//
// INTEGRACIÓN:
//   - Program.cs: app.MapHub<Manda2Hub>("/hubs/manda2")
//   - MAUI: HubConnectionBuilder → wss://api/hubs/manda2
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.DTOs;
using Manda2.Application.Services;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Manda2.API.Hubs
{
    /// <summary>
    /// Hub central de SignalR. Un solo hub para todos los actores.
    /// Cada actor se une a los grupos que le corresponden al conectar.
    /// </summary>
    [Authorize]
    public class Manda2Hub : Hub
    {
        private readonly IDriverLocationCache _locationCache;
        private readonly IApplicationDbContext _db;

        public Manda2Hub(IDriverLocationCache locationCache, IApplicationDbContext db)
        {
            _locationCache = locationCache;
            _db = db;
        }

        // ═══════════════════════════════════════════════════════════════════
        // CICLO DE VIDA DE CONEXIÓN
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Al conectar: el cliente se une automáticamente a sus grupos
        /// según el rol declarado en el JWT.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirst(AppClaims.Role)?.Value;

            switch (role)
            {
                case AppRoles.Driver:
                    int driverId = GetDriverId();
                    if (driverId > 0)
                    {
                        // Grupo personal del driver
                        await Groups.AddToGroupAsync(
                            Context.ConnectionId, $"driver_{driverId}");

                        // Si tiene un grupo activo, unirse al canal del grupo
                        var activeGroupId = await _db.Drivers
                            .Where(d => d.Id == driverId)
                            .Select(d => d.CurrentOrderGroupId)
                            .FirstOrDefaultAsync();

                        if (activeGroupId.HasValue)
                            await Groups.AddToGroupAsync(
                                Context.ConnectionId, $"group_{activeGroupId.Value}");
                    }
                    break;

                case AppRoles.Customer:
                    int customerId = GetCustomerId();
                    if (customerId > 0)
                        await Groups.AddToGroupAsync(
                            Context.ConnectionId, $"customer_{customerId}");
                    break;

                case AppRoles.Merchant:
                    int merchantId = GetMerchantId();
                    if (merchantId > 0)
                        await Groups.AddToGroupAsync(
                            Context.ConnectionId, $"merchant_{merchantId}");
                    break;

                case AppRoles.BackOffice:
                case AppRoles.Admin:
                    await Groups.AddToGroupAsync(Context.ConnectionId, "backoffice");
                    break;
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Al desconectar: limpiar caché GPS si es un driver.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var role = Context.User?.FindFirst(AppClaims.Role)?.Value;

            if (role == AppRoles.Driver)
            {
                int driverId = GetDriverId();
                if (driverId > 0)
                    await _locationCache.RemoveAsync(driverId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ═══════════════════════════════════════════════════════════════════
        // CANAL 1: GPS DEL DRIVER
        // Llamado por la app del driver periódicamente (cada 5 seg aprox).
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Driver envía su posición GPS.
        /// El hub actualiza el caché y hace broadcast al grupo activo.
        ///
        /// Invocado desde MAUI: await hubConnection.InvokeAsync("UpdateLocation", lat, lon)
        /// Recibido en MAUI (cliente/backoffice): hubConnection.On("DriverLocationUpdated", ...)
        /// </summary>
        /// <param name="latitude">Latitud actual del driver.</param>
        /// <param name="longitude">Longitud actual del driver.</param>
        public async Task UpdateLocation(decimal latitude, decimal longitude)
        {
            // ── Validar que el caller es un Driver ────────────────────────
            int driverId = GetDriverId();
            if (driverId == 0)
            {
                await Clients.Caller.SendAsync("Error", "Token no contiene DriverId.");
                return;
            }

            // ── Actualizar caché GPS ──────────────────────────────────────
            var location = new DriverLocationDto
            {
                DriverId = driverId,
                Latitude = latitude,
                Longitude = longitude,
                UpdatedAt = DateTime.UtcNow
            };

            await _locationCache.SetAsync(driverId, location);

            // ── Broadcast al grupo activo del driver ──────────────────────
            // Todos los conectados al grupo (cliente, comercio, backoffice)
            // reciben la posición actualizada.
            var activeGroupId = await _db.Drivers
                .Where(d => d.Id == driverId)
                .Select(d => d.CurrentOrderGroupId)
                .FirstOrDefaultAsync();

            if (activeGroupId.HasValue)
            {
                await Clients
                    .Group($"group_{activeGroupId.Value}")
                    .SendAsync("DriverLocationUpdated", location);
            }

            // ── Broadcast a BackOffice siempre ────────────────────────────
            await Clients
                .Group("backoffice")
                .SendAsync("DriverLocationUpdated", location);
        }

        // ═══════════════════════════════════════════════════════════════════
        // CANAL 2: SUSCRIPCIÓN A UN GRUPO DE PEDIDO
        // El cliente se une al canal del grupo para recibir tracking.
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Cliente o Comercio se suscribe al canal de un OrderGroup.
        /// Valida que el caller tenga relación con ese grupo.
        ///
        /// Invocado desde MAUI: await hubConnection.InvokeAsync("JoinOrderGroup", groupId)
        /// </summary>
        /// <param name="orderGroupId">ID del OrderGroup a seguir.</param>
        public async Task JoinOrderGroup(int orderGroupId)
        {
            var role = Context.User?.FindFirst(AppClaims.Role)?.Value;

            // ── Validar ownership según rol ───────────────────────────────
            bool authorized = false;

            switch (role)
            {
                case AppRoles.Customer:
                    int customerId = GetCustomerId();
                    authorized = await _db.OrderGroups
                        .AnyAsync(g => g.Id == orderGroupId && g.CustomerId == customerId);
                    break;

                case AppRoles.Merchant:
                    int merchantId = GetMerchantId();
                    // El comercio tiene al menos una SubOrder en este grupo
                    authorized = await _db.SubOrders
                        .AnyAsync(s => s.OrderGroupId == orderGroupId &&
                                       s.MerchantId == merchantId);
                    break;

                case AppRoles.Driver:
                    int driverId = GetDriverId();
                    authorized = await _db.OrderGroups
                        .AnyAsync(g => g.Id == orderGroupId && g.DriverId == driverId);
                    break;

                case AppRoles.BackOffice:
                case AppRoles.Admin:
                    authorized = true;
                    break;
            }

            if (!authorized)
            {
                await Clients.Caller.SendAsync("Error", "No autorizado para este grupo.");
                return;
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"group_{orderGroupId}");

            // Enviar última posición conocida del driver (si existe)
            var group = await _db.OrderGroups
                .Where(g => g.Id == orderGroupId)
                .Select(g => new { g.DriverId })
                .FirstOrDefaultAsync();

            if (group?.DriverId != null)
            {
                var lastLocation = await _locationCache.GetAsync(group.DriverId.Value);
                if (lastLocation != null)
                    await Clients.Caller.SendAsync("DriverLocationUpdated", lastLocation);
            }
        }

        /// <summary>
        /// Cliente o Comercio abandona el canal de un OrderGroup.
        /// Invocado desde MAUI: await hubConnection.InvokeAsync("LeaveOrderGroup", groupId)
        /// </summary>
        public async Task LeaveOrderGroup(int orderGroupId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, $"group_{orderGroupId}");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CANAL 3: NOTIFICACIONES DE ESTADO (server → client)
        // Estos métodos son llamados desde los HANDLERS, no desde el cliente.
        // Se exponen aquí como referencia de los eventos que el cliente
        // debe escuchar en MAUI.
        //
        // Eventos que el cliente MAUI debe registrar:
        //   "OrderGroupStatusChanged"  → estado del grupo cambió
        //   "SubOrderStatusChanged"    → estado de una suborden cambió
        //   "DispatchOfferReceived"    → driver recibe nueva oferta
        //   "DriverLocationUpdated"    → posición del driver actualizada
        //   "Error"                    → error del servidor
        // ═══════════════════════════════════════════════════════════════════

        // ─── Helpers privados ─────────────────────────────────────────────

        private int GetDriverId()
        {
            var claim = Context.User?.FindFirst(AppClaims.DriverId)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private int GetCustomerId()
        {
            var claim = Context.User?.FindFirst(AppClaims.CustomerId)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private int GetMerchantId()
        {
            var claim = Context.User?.FindFirst(AppClaims.MerchantId)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}

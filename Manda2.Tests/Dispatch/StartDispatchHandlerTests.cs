// PROPÓSITO:
//   Tests de integración liviana para StartDispatchCommandHandler.
//   Usa EF Core InMemory para simular la base de datos sin SQL Server real.
//
// PUNTO DE REFERENCIA GEOGRÁFICO:
//   Comercio ficticio: "Restaurante El Pino, Jinotega, Nicaragua"
//   Lat: 13.0924 | Lon: -85.9990
//

using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Manda2.Infrastructure.Notifications;
using Manda2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Tests.Dispatch
{
    public class StartDispatchHandlerTests
    {
        // ─── PUNTO DE REFERENCIA ──────────────────────────────────────────────
        // Comercio ficticio: "Restaurante El Pino, Jinotega, Nicaragua"
        // Coordenadas del centro de Jinotega
        private const double RefLat = 13.0924;
        private const double RefLon = -85.9990;

        // ─── HELPERS ──────────────────────────────────────────────────────────

        /// <summary>
        /// Crea un DbContext InMemory con nombre único por test para evitar colisiones.
        /// Cada test tiene su propia base de datos aislada en memoria.
        /// </summary>
        private static ApplicationDbContext CreateDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        /// <summary>
        /// Crea un driver con posición GPS específica relativa a Jinotega.
        /// Por defecto el driver está disponible (IsOnline=true, Status=Available).
        /// </summary>
        private static Driver MakeDriver(int id, double lat, double lon, bool available = true)
            => new Driver
            {
                Id = id,
                FirstName = $"Driver{id}",
                LastName = "Test",
                Email = $"driver{id}@test.com",
                PhoneNumber = "00000000",
                IdentityDocumentUrl = "http://test.com/doc.pdf",
                IsActive = true,
                IsOnline = available,
                Status = available ? DriverStatus.Available : DriverStatus.Offline,
                CurrentOrderGroupId = null,
                LastLatitude = (decimal)lat,
                LastLongitude = (decimal)lon,
                LastLocationUpdateAt = DateTime.UtcNow
            };

        /// <summary>
        /// Crea un OrderGroup mínimo con un Pickup stop en las coordenadas
        /// del comercio de referencia (Jinotega).
        /// Usa Sequence (no SortOrder) alineado con OrderGroupStop.Sequence.
        /// </summary>
        private static OrderGroup MakeOrderGroup(int id)
            => new OrderGroup
            {
                Id = id,
                CustomerId = 1,
                Status = OrderGroupStatus.AwaitingDriverAssignment,
                DeliveryAddressText = "Barrio El Calvario, Jinotega, Nicaragua",
                DeliveryLatitude = (decimal)RefLat,
                DeliveryLongitude = (decimal)RefLon,
                Stops = new List<OrderGroupStop>
                {
                    new OrderGroupStop
                    {
                        Id         = id * 10,          // ID único por grupo para evitar colisiones
                        StopType   = StopType.Pickup,
                        Latitude   = (decimal)RefLat,
                        Longitude  = (decimal)RefLon,
                        Sequence   = 1,                // ✅ Sequence (no SortOrder)
                        AddressText = "Restaurante El Pino, Jinotega"
                    }
                }
            };

        /// <summary>
        /// Crea la configuración de dispatch por defecto desde AppConfig.
        /// Estos valores replican el Seed Data de ApplicationDbContext.
        /// </summary>
        private static List<AppConfig> DefaultAppConfigs()
    => new List<AppConfig>
    {
        new AppConfig
        {
            Id          = 1,
            Key         = "DISPATCH_DEFAULT_MAX_DRIVERS_PER_ROUND",
            Value       = "5",
            Description = "Máximo de drivers notificados por ronda de dispatch"
        },
        new AppConfig
        {
            Id          = 2,
            Key         = "DISPATCH_DEFAULT_INITIAL_RADIUS_KM",
            Value       = "3",
            Description = "Radio inicial en km para búsqueda de drivers"
        },
        new AppConfig
        {
            Id          = 3,
            Key         = "DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES",
            Value       = "3",
            Description = "Minutos antes de expirar una ronda de dispatch"
        }
    };

        // ═══════════════════════════════════════════════════════════════════════
        // TEST 1
        // Escenario : 3 drivers, 2 dentro del radio (≤3km), 1 fuera (>3km).
        // Esperado  : DispatchAttempt creado con 2 DispatchAttemptDriver.
        //
        // Drivers (referencia: Jinotega 13.0924, -85.9990):
        //   D1 → 13.1059, -85.9990  ≈ 1.5 km norte  ✅ DENTRO
        //   D2 → 13.0672, -85.9990  ≈ 2.8 km sur    ✅ DENTRO
        //   D3 → 13.1644, -85.9990  ≈ 8.0 km norte  ❌ FUERA
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task StartDispatch_Should_Notify_Only_Drivers_Within_Radius()
        {
            // Arrange
            await using var db = CreateDb(nameof(StartDispatch_Should_Notify_Only_Drivers_Within_Radius));

            var drivers = new[]
            {
                MakeDriver(1, 13.1059, -85.9990),  // ~1.5 km norte  ✅ DENTRO radio 3km
                MakeDriver(2, 13.0672, -85.9990),  // ~2.8 km sur    ✅ DENTRO radio 3km
                MakeDriver(3, 13.1644, -85.9990)   // ~8.0 km norte  ❌ FUERA  radio 3km
            };

            var group = MakeOrderGroup(10);

            db.Drivers.AddRange(drivers);
            db.OrderGroups.Add(group);
            db.AppConfigs.AddRange(DefaultAppConfigs());
            await db.SaveChangesAsync();

            var stub = new NotificationServiceStub(NullLogger<NotificationServiceStub>.Instance);
            var handler = new StartDispatchCommandHandler(db, stub);
            var command = new StartDispatchCommand(orderGroupId: 10);

            // Act
            var result = await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.NotifiedDriversCount);
            Assert.True(result.AttemptId > 0);

            var attempt = await db.DispatchAttempts.FindAsync(result.AttemptId);
            Assert.NotNull(attempt);

            // ✅ Sent (no InProgress — corregido)
            Assert.Equal(DispatchAttemptStatus.Sent, attempt!.Status);

            var notified = await db.DispatchAttemptDrivers
                .Where(d => d.DispatchAttemptId == result.AttemptId)
                .ToListAsync();

            Assert.Equal(2, notified.Count);
            Assert.All(notified, d => Assert.Equal(DriverDispatchResponse.Pending, d.Response));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // TEST 2
        // Escenario : Sin drivers disponibles en el radio (≤3km).
        // Esperado  : DispatchAttempt creado con NotifiedDriversCount = 0.
        //
        // Drivers (referencia: Jinotega 13.0924, -85.9990):
        //   D1 → 13.1800, -85.9990  ≈ 9.7 km norte  ❌ FUERA (único driver)
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task StartDispatch_Should_Create_Attempt_Even_With_No_Drivers()
        {
            // Arrange
            await using var db = CreateDb(nameof(StartDispatch_Should_Create_Attempt_Even_With_No_Drivers));

            // Driver único muy lejos (~9.7 km norte de Jinotega)
            db.Drivers.Add(MakeDriver(1, 13.1800, -85.9990));
            db.OrderGroups.Add(MakeOrderGroup(20));
            db.AppConfigs.AddRange(DefaultAppConfigs());
            await db.SaveChangesAsync();

            var stub = new NotificationServiceStub(NullLogger<NotificationServiceStub>.Instance);
            var handler = new StartDispatchCommandHandler(db, stub);
            var command = new StartDispatchCommand(orderGroupId: 20);

            // Act
            var result = await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            Assert.Equal(0, result.NotifiedDriversCount);
            Assert.True(result.AttemptId > 0);
            Assert.Contains("no hay drivers", result.Message, StringComparison.OrdinalIgnoreCase);

            var notified = await db.DispatchAttemptDrivers
                .Where(d => d.DispatchAttemptId == result.AttemptId)
                .ToListAsync();

            Assert.Empty(notified);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // TEST 3
        // Escenario : 4 drivers dentro del radio (≤5km), MaxDriversPerRound=2.
        // Esperado  : Solo se notifican los 2 más cercanos al comercio.
        //
        // Drivers (referencia: Jinotega 13.0924, -85.9990):
        //   D1 → 13.1059, -85.9990  ≈ 1.5 km norte  ✅ DENTRO — más cercano
        //   D2 → 13.0672, -85.9990  ≈ 2.8 km sur    ✅ DENTRO — segundo más cercano
        //   D3 → 13.1200, -85.9990  ≈ 3.1 km norte  ✅ DENTRO — tercero
        //   D4 → 13.1350, -85.9990  ≈ 4.7 km norte  ✅ DENTRO — cuarto
        //
        // Con MaxDriversPerRound=2 → solo D1 y D2 deben ser notificados.
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task StartDispatch_Should_Respect_MaxDriversPerRound()
        {
            // Arrange
            await using var db = CreateDb(nameof(StartDispatch_Should_Respect_MaxDriversPerRound));

            // AppConfig con max=2 y radio=5km para que los 4 drivers queden dentro
            db.AppConfigs.AddRange(new[]
            {
                new AppConfig { Id = 1, Key = "DISPATCH_DEFAULT_MAX_DRIVERS_PER_ROUND",  Value = "2", Description = "Max drivers por ronda" },
                new AppConfig { Id = 2, Key = "DISPATCH_DEFAULT_INITIAL_RADIUS_KM",       Value = "5", Description = "Radio inicial km" },
                new AppConfig { Id = 3, Key = "DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES",   Value = "3", Description = "Timeout ronda minutos" }
            });

            // 4 drivers dentro del radio de 5km desde Jinotega
            db.Drivers.AddRange(new[]
            {
                MakeDriver(1, 13.1059, -85.9990),  // ~1.5 km norte ✅
                MakeDriver(2, 13.0672, -85.9990),  // ~2.8 km sur   ✅
                MakeDriver(3, 13.1200, -85.9990),  // ~3.1 km norte ✅
                MakeDriver(4, 13.1350, -85.9990)   // ~4.7 km norte ✅
            });

            db.OrderGroups.Add(MakeOrderGroup(30));
            await db.SaveChangesAsync();

            var stub = new NotificationServiceStub(NullLogger<NotificationServiceStub>.Instance);
            var handler = new StartDispatchCommandHandler(db, stub);
            var command = new StartDispatchCommand(orderGroupId: 30);

            // Act
            var result = await handler.HandleAsync(command, CancellationToken.None);

            // Assert: solo 2 notificados (los más cercanos al comercio)
            Assert.Equal(2, result.NotifiedDriversCount);

            var notified = await db.DispatchAttemptDrivers
                .Where(d => d.DispatchAttemptId == result.AttemptId)
                .OrderBy(d => d.DistanceKm)
                .ToListAsync();

            Assert.Equal(2, notified.Count);

            // El primero debe ser el más cercano (DistanceKm menor)
            Assert.True(notified[0].DistanceKm <= notified[1].DistanceKm);

            // Verificar que D1 (~1.5km) y D2 (~2.8km) son los notificados
            var notifiedDriverIds = notified.Select(n => n.DriverId).ToHashSet();
            Assert.Contains(1, notifiedDriverIds); // D1 más cercano
            Assert.Contains(2, notifiedDriverIds); // D2 segundo más cercano
        }
    }
}

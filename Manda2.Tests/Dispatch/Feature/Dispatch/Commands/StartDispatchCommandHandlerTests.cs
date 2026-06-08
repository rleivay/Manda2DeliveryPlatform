using FluentAssertions;
using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Manda2.Tests.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Manda2.Contracts.Enum.DispatchEnums;

// PROPÓSITO:
//   Pruebas unitarias para StartDispatchCommandHandler.
//   Cubre: camino feliz con drivers disponibles, sin drivers en radio,
//          grupo no encontrado, estado inválido del grupo,
//          configuración por zona, configuración por AppConfig,
//          fallback a constantes, creación de DispatchAttempt y
//          DispatchAttemptDriver, registro de AuditLog.
//
// DEPENDENCIAS MOCKEADAS:
//   - IApplicationDbContext  → acceso a BD (EF Core)
//   - INotificationService   → notificaciones push/SignalR (stub)
//   - MockDbSetHelper        → helper compartido para DbSet en memoria

namespace Manda2.Tests.Dispatch.Feature.Dispatch.Commands
{
    public class StartDispatchCommandHandlerTests
    {
        // ─── DEPENDENCIAS ────
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly Mock<INotificationService> _mockNotification;
        private readonly StartDispatchCommandHandler _handler;

        // ─── IDs FIJOS ────
        private const int OrderGroupId = 200;
        private const int DriverId1 = 20;
        private const int DriverId2 = 21;

        // ─── COORDENADAS DE REFERENCIA (Zona 10, Nicaragua) ────
        private const decimal RefLat = 14.6099m;
        private const decimal RefLon = -90.5133m;

        public StartDispatchCommandHandlerTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockNotification = new Mock<INotificationService>();
            _handler = new StartDispatchCommandHandler(
                _mockContext.Object,
                _mockNotification.Object);
        }

        // ════════════════════════════════════════════════════════════════════
        // HELPERS PRIVADOS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un OrderGroup en estado AwaitingDriverAssignment con un Pickup stop.
        /// </summary>
        private static OrderGroup BuildGroupAwaitingDispatch(int id = OrderGroupId) => new()
        {
            Id = id,
            CustomerId = 1,
            Status = OrderGroupStatus.AwaitingDriverAssignment,
            DeliveryAddressText = "Zona 10, Nicaragua",
            DeliveryLatitude = RefLat,
            DeliveryLongitude = RefLon,
            SubOrderCount = 1,
            DeliveryFee = 15m,
            ServiceFee = 5m,
            TotalAmount = 120m,
            SubOrders = new List<SubOrder>
            {
                new() { Id = 1, MerchantId = 1}
            },
            Stops = new List<OrderGroupStop>
            {
                new()
                {
                    Id          = 1,
                    OrderGroupId = id,
                    StopType    = StopType.Pickup,
                    Sequence    = 1,
                    AddressText = "Restaurante Central, Zona 1",
                    Latitude    = RefLat,
                    Longitude   = RefLon,
                    MerchantId  = 1
                }
            }
        };

        /// <summary>
        /// Crea un Driver disponible con GPS dentro del radio de 3km del punto de referencia.
        /// </summary>
        private static Driver BuildNearbyDriver(int id = DriverId1) => new()
        {
            Id = id,
            FirstName = "Carlos",
            LastName = "López",
            Email = $"driver{id}@manda2.com",
            PhoneNumber = "50299887766",
            IdentityDocumentUrl = "https://docs.manda2.com/id/carlos.pdf",
            IsActive = true,
            IsOnline = true,
            Status = DriverStatus.Available,
            CurrentOrderGroupId = null,
            MaxActiveGroups = 3,
            MaxSubOrderLimit = 5,
            MaxCashLimit = 500m,
            CurrentCashBalance = 0m,
            IsCashBlocked = false,
            // GPS a ~0.5km del punto de referencia → dentro del radio de 3km
            LastLatitude = RefLat + 0.004m,
            LastLongitude = RefLon + 0.004m,
            LastLocationUpdateAt = DateTime.UtcNow.AddMinutes(-2)
        };

        /// <summary>
        /// Crea un Driver fuera del radio de dispatch (>10km del punto de referencia).
        /// </summary>
        private static Driver BuildFarDriver(int id = 99) => new()
        {
            Id = id,
            FirstName = "Pedro",
            LastName = "Lejano",
            Email = $"far{id}@manda2.com",
            PhoneNumber = "50211112222",
            IdentityDocumentUrl = "https://docs.manda2.com/id/pedro.pdf",
            IsActive = true,
            IsOnline = true,
            Status = DriverStatus.Available,
            CurrentOrderGroupId = null,
            MaxActiveGroups = 3,
            MaxSubOrderLimit = 5,
            MaxCashLimit = 500m,
            CurrentCashBalance = 0m,
            IsCashBlocked = false,
            // GPS a ~15km del punto de referencia → fuera del radio
            LastLatitude = RefLat + 0.135m,
            LastLongitude = RefLon + 0.135m,
            LastLocationUpdateAt = DateTime.UtcNow.AddMinutes(-1)
        };

        /// <summary>
        /// Configura el contexto con los datos mínimos para que el handler funcione.
        /// Usa AppConfig con valores por defecto (sin zona específica).
        /// </summary>
        private void SetupMinimalContext(
            OrderGroup group,
            List<Driver> drivers,
            List<DispatchAttempt>? attempts = null,
            List<DispatchAttemptDriver>? attemptDrivers = null,
            List<AppConfig>? appConfigs = null,
            List<DispatchConfig>? dispatchConfigs = null)
        {
            _mockContext.Setup(c => c.OrderGroups)
                .Returns(MockDbSetHelper.CreateMockDbSet(new List<OrderGroup> { group }).Object);

            _mockContext.Setup(c => c.Drivers)
                .Returns(MockDbSetHelper.CreateMockDbSet(drivers).Object);

            _mockContext.Setup(c => c.DispatchAttempts)
                .Returns(MockDbSetHelper.CreateMockDbSet(attempts ?? new List<DispatchAttempt>()).Object);

            _mockContext.Setup(c => c.DispatchAttemptDrivers)
                .Returns(MockDbSetHelper.CreateMockDbSet(attemptDrivers ?? new List<DispatchAttemptDriver>()).Object);

            _mockContext.Setup(c => c.AppConfigs)
                .Returns(MockDbSetHelper.CreateMockDbSet(appConfigs ?? new List<AppConfig>()).Object);

            _mockContext.Setup(c => c.DispatchConfigs)
                .Returns(MockDbSetHelper.CreateMockDbSet(dispatchConfigs ?? new List<DispatchConfig>()).Object);

            var mockAuditLogs = MockDbSetHelper.CreateMockDbSet(new List<AuditLog>());
            _mockContext.Setup(c => c.AuditLogs).Returns(mockAuditLogs.Object);
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 1 — CAMINO FELIZ: DRIVERS DISPONIBLES EN RADIO
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: Grupo válido, 2 drivers dentro del radio, AppConfig con defaults.
        /// Esperado:
        ///   - AttemptId > 0
        ///   - NotifiedDriversCount = 2
        ///   - ExpiresAt > UtcNow
        ///   - SaveChangesAsync llamado 3 veces (attempt, attemptDrivers, auditLog)
        ///   - NotifyDriverDispatchOfferAsync llamado 2 veces
        /// </summary>
        [Fact]
        public async Task HandleAsync_DriversAvailableInRadius_ShouldCreateAttemptAndNotify()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            var driver1 = BuildNearbyDriver(DriverId1);
            var driver2 = BuildNearbyDriver(DriverId2);
            driver2.Email = "driver2@manda2.com";

            SetupMinimalContext(group, new List<Driver> { driver1, driver2 });

            var command = new StartDispatchCommand(OrderGroupId);

            // Act
            var result = await _handler.HandleAsync(command, CancellationToken.None);

            // Assert — resultado
            result.Should().NotBeNull();
            result.NotifiedDriversCount.Should().Be(2);
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            result.Message.Should().Contain("2 driver(s) notificados");

            // Assert — persistencia (3 SaveChanges: attempt + attemptDrivers + auditLog)
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));

            // Assert — notificaciones enviadas a ambos drivers
            _mockNotification.Verify(n => n.NotifyDriverDispatchOfferAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 2 — SIN DRIVERS EN RADIO
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: No hay drivers disponibles dentro del radio configurado.
        /// Esperado:
        ///   - NotifiedDriversCount = 0
        ///   - Mensaje indica que no hay drivers disponibles
        ///   - DispatchAttempt creado igualmente (para registro de la ronda)
        ///   - NotifyDriverDispatchOfferAsync NO llamado
        /// </summary>
        [Fact]
        public async Task HandleAsync_NoDriversInRadius_ShouldReturnZeroNotified()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            var farDriver = BuildFarDriver();

            SetupMinimalContext(group, new List<Driver> { farDriver });

            var command = new StartDispatchCommand(OrderGroupId);

            // Act
            var result = await _handler.HandleAsync(command, CancellationToken.None);

            // Assert
            result.NotifiedDriversCount.Should().Be(0);
            result.Message.Should().Contain("no hay drivers disponibles");

            _mockNotification.Verify(n => n.NotifyDriverDispatchOfferAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 3 — GRUPO NO ENCONTRADO
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: OrderGroupId no existe en la BD.
        /// Esperado: InvalidOperationException con mensaje descriptivo.
        /// </summary>
        [Fact]
        public async Task HandleAsync_GroupNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange — contexto sin grupos
            SetupMinimalContext(
                group: BuildGroupAwaitingDispatch(), // no importa, no se encontrará
                drivers: new List<Driver>());

            _mockContext.Setup(c => c.OrderGroups)
                .Returns(MockDbSetHelper.CreateMockDbSet(new List<OrderGroup>()).Object);

            var command = new StartDispatchCommand(9999);

            // Act & Assert
            await FluentActions
                .Invoking(() => _handler.HandleAsync(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*no encontrado*");
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 4 — ESTADO INVÁLIDO DEL GRUPO
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: El grupo existe pero no está en AwaitingDriverAssignment.
        ///            (Ej: ya fue asignado o está cancelado.)
        /// Esperado: InvalidOperationException indicando estado inválido.
        /// </summary>
        [Fact]
        public async Task HandleAsync_GroupInvalidStatus_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            group.Status = OrderGroupStatus.DriverAccepted; // Estado incorrecto

            SetupMinimalContext(group, new List<Driver>());

            var command = new StartDispatchCommand(OrderGroupId);

            // Act & Assert
            await FluentActions
                .Invoking(() => _handler.HandleAsync(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*no está en estado válido*");
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 5 — CONFIGURACIÓN POR ZONA
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: Se pasa ZoneName y existe un DispatchConfig para esa zona.
        /// Esperado: El handler usa el radio de la zona (no el de AppConfig).
        ///           Con radio = 1km, el driver a 0.5km debe ser notificado.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithZoneConfig_ShouldUseZoneRadius()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            var driver = BuildNearbyDriver(); // ~0.5km del punto de referencia

            var zoneConfig = new DispatchConfig
            {
                Id = 1,
                ZoneName = "Zona10",
                InitialRadiusKm = 1.0m,  // Radio pequeño: 1km
                RadiusExpansionFactor = 2.0m,
                MaxDriversToNotifyPerRound = 5,
                RoundTimeoutMinutes = 3,
                MaxRounds = 3,
                IsActive = true
            };

            SetupMinimalContext(
                group,
                new List<Driver> { driver },
                dispatchConfigs: new List<DispatchConfig> { zoneConfig });

            var command = new StartDispatchCommand(OrderGroupId, zoneName: "Zona10");

            // Act
            var result = await _handler.HandleAsync(command, CancellationToken.None);

            // Assert — el driver a 0.5km debe estar dentro del radio de 1km
            result.NotifiedDriversCount.Should().Be(1);
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 6 — CONFIGURACIÓN POR APPCONFIG
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: No hay ZoneName, pero AppConfig tiene los valores configurados.
        /// Esperado: El handler usa los valores de AppConfig.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithAppConfig_ShouldUseAppConfigValues()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            var driver = BuildNearbyDriver();

            var appConfigs = new List<AppConfig>
            {
                new() { Id = 1, Key = "DISPATCH_DEFAULT_INITIAL_RADIUS_KM",       Value = "5" },
                new() { Id = 2, Key = "DISPATCH_DEFAULT_MAX_DRIVERS_PER_ROUND",    Value = "3" },
                new() { Id = 3, Key = "DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES",    Value = "4" }
            };

            SetupMinimalContext(group, new List<Driver> { driver }, appConfigs: appConfigs);

            var command = new StartDispatchCommand(OrderGroupId);

            // Act
            var result = await _handler.HandleAsync(command, CancellationToken.None);

            // Assert — driver dentro del radio de 5km debe ser notificado
            result.NotifiedDriversCount.Should().Be(1);
            // ExpiresAt debe ser ~4 minutos desde ahora (timeout de AppConfig)
            result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(4), precision: TimeSpan.FromSeconds(5));
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 7 — FALLBACK A CONSTANTES
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: Sin ZoneName y sin AppConfig configurado.
        /// Esperado: El handler usa las constantes internas (radio=3km, timeout=3min).
        /// </summary>
        [Fact]
        public async Task HandleAsync_NoConfig_ShouldUseFallbackConstants()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            var driver = BuildNearbyDriver(); // ~0.5km → dentro del radio de 3km

            // Sin AppConfig ni DispatchConfig
            SetupMinimalContext(group, new List<Driver> { driver });

            var command = new StartDispatchCommand(OrderGroupId);

            // Act
            var result = await _handler.HandleAsync(command, CancellationToken.None);

            // Assert — driver dentro del radio de 3km (fallback) debe ser notificado
            result.NotifiedDriversCount.Should().Be(1);
            // ExpiresAt debe ser ~3 minutos desde ahora (timeout fallback)
            result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(3), precision: TimeSpan.FromSeconds(5));
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 8 — GRUPO SIN COORDENADAS VÁLIDAS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: El grupo no tiene Stops ni coordenadas de entrega.
        /// Esperado: InvalidOperationException indicando falta de coordenadas.
        /// </summary>
        [Fact]
        public async Task HandleAsync_GroupWithNoCoordinates_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            group.Stops = new List<OrderGroupStop>(); // Sin stops
            group.DeliveryLatitude = 0m;
            group.DeliveryLongitude = 0m;

            SetupMinimalContext(group, new List<Driver>());

            var command = new StartDispatchCommand(OrderGroupId);

            // Act & Assert
            await FluentActions
                .Invoking(() => _handler.HandleAsync(command, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*coordenadas válidas*");
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 9 — AUDITLOG REGISTRADO
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: Dispatch iniciado exitosamente.
        /// Esperado: Se agrega exactamente 1 AuditLog con Action = "StartDispatch".
        /// </summary>
        [Fact]
        public async Task HandleAsync_SuccessfulDispatch_ShouldAddAuditLog()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            var driver = BuildNearbyDriver();
            var auditLogs = new List<AuditLog>();

            SetupMinimalContext(group, new List<Driver> { driver });

            // Capturar el Add del AuditLog
            var mockAuditSet = MockDbSetHelper.CreateMockDbSet(auditLogs);
            mockAuditSet.Setup(s => s.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(log => auditLogs.Add(log));
            _mockContext.Setup(c => c.AuditLogs).Returns(mockAuditSet.Object);

            var command = new StartDispatchCommand(OrderGroupId, initiatedByUserId: 1);

            // Act
            await _handler.HandleAsync(command, CancellationToken.None);

            // Assert
            auditLogs.Should().HaveCount(1);
            auditLogs[0].Action.Should().Be("StartDispatch");
            auditLogs[0].EntityName.Should().Be(nameof(OrderGroup));
            auditLogs[0].EntityId.Should().Be(OrderGroupId);
        }

        // ════════════════════════════════════════════════════════════════════
        // TEST 10 — DRIVER OCUPADO NO ELEGIBLE
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Escenario: El único driver en radio está Busy (CurrentOrderGroupId != null).
        /// Esperado: NotifiedDriversCount = 0 (no se notifica a drivers no elegibles).
        /// </summary>
        [Fact]
        public async Task HandleAsync_DriverBusyInRadius_ShouldNotNotify()
        {
            // Arrange
            var group = BuildGroupAwaitingDispatch();
            var driver = BuildNearbyDriver();
            driver.Status = DriverStatus.Busy;
            driver.CurrentOrderGroupId = 999; // Ya tiene un grupo activo

            SetupMinimalContext(group, new List<Driver> { driver });

            var command = new StartDispatchCommand(OrderGroupId);

            // Act
            var result = await _handler.HandleAsync(command, CancellationToken.None);

            // Assert
            result.NotifiedDriversCount.Should().Be(0);
            _mockNotification.Verify(n => n.NotifyDriverDispatchOfferAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

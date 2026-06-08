// PROPÓSITO:
//   Suite de pruebas unitarias para CompleteStopCommandHandler.
//   Cubre todas las reglas de negocio definidas en el handler:
//     1. Idempotencia
//     2. Seguridad (driver no asignado)
//     3. Parada no encontrada
//     4. Pickup → SubOrder.PickedUp
//     5. Pickup último → OrderGroup.InRoute
//     6. Pickup con pickups pendientes → OrderGroup NO cambia a InRoute
//     7. Dropoff → SubOrders.Delivered
//     8. Dropoff → OrderGroup.Delivered + Driver liberado
//     9. Dropoff parcial (multi-Dropoff) → grupo NO se cierra
//    10. AuditLog registrado correctamente
//    11. Notificaciones disparadas post-SaveChanges
//    12. SubOrder no encontrada para Pickup → excepción
//
// PATRÓN:
//   Arrange / Act / Assert (AAA) estricto.
//   Mocks con Moq para IApplicationDbContext e INotificationService.
//   InMemory DbContext NO se usa — se mockea a nivel de interfaz para
//   mantener las pruebas rápidas y sin dependencia de EF Core InMemory.
//
//   EXCEPCIÓN: Para los casos donde EF Core materializa la query con
//   FirstOrDefaultAsync + Include, usamos un helper que construye
//   DbSet<T> mockeado con datos en memoria (MockDbSetHelper).
//
using FluentAssertions;
using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Manda2.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Tests.Dispatch.Feature.Dispatch.Commands
{
    public class CompleteStopCommandHandlerTests
    {
        // ─── FIXTURES COMPARTIDOS ────────────────────────────────────────────────
        // Constantes reutilizadas en todos los tests para evitar magic numbers.
        private const int DriverId = 10;
        private const int OtherDriverId = 99;
        private const int GroupId = 1;
        private const int MerchantId = 5;
        private const int CustomerId = 20;

        // ─── HELPERS DE CONSTRUCCIÓN ─────────────────────────────────────────────

        /// <summary>
        /// Construye un OrderGroup con 1 Pickup y 1 Dropoff, y 1 SubOrder.
        /// Configura las relaciones de navegación en memoria para que EF
        /// las resuelva correctamente en el mock.
        /// </summary>
        private static (OrderGroup group, OrderGroupStop pickupStop, OrderGroupStop dropoffStop, SubOrder subOrder)
            BuildSingleMerchantGroup(
                int stopId,
                StopType stopType,
                SubOrderStatus subOrderStatus = SubOrderStatus.Delivered)
        {
            var subOrder = new SubOrder
            {
                Id = 1,
                MerchantId = MerchantId,
                Status = subOrderStatus
            };

            var pickupStop = new OrderGroupStop
            {
                Id = stopId,
                StopType = StopType.Pickup,
                MerchantId = MerchantId,
                CompletedAt = stopType == StopType.Pickup ? null : DateTime.UtcNow.AddMinutes(-5)
            };

            var dropoffStop = new OrderGroupStop
            {
                Id = stopId + 100,
                StopType = StopType.Dropoff,
                MerchantId = null,
                CompletedAt = null
            };

            var group = new OrderGroup
            {
                Id = GroupId,
                DriverId = DriverId,
                CustomerId = CustomerId,
                Status = OrderGroupStatus.DriverAccepted,
                SubOrders = new List<SubOrder> { subOrder },
                Stops = new List<OrderGroupStop> { pickupStop, dropoffStop }
            };

            // Enlazar navegación inversa
            pickupStop.OrderGroup = group;
            dropoffStop.OrderGroup = group;

            return (group, pickupStop, dropoffStop, subOrder);
        }

        /// <summary>
        /// Construye el handler con mocks configurados para el stop dado.
        /// Retorna el handler, el mock del DbContext y el mock de notificaciones.
        /// </summary>
        private static (
            CompleteStopCommandHandler handler,
            Mock<IApplicationDbContext> dbMock,
            Mock<INotificationService> notifMock)
            BuildHandler(OrderGroupStop targetStop, Driver? driver = null)
        {
            var dbMock = new Mock<IApplicationDbContext>();
            var notifMock = new Mock<INotificationService>();

            // Mock de OrderGroupStops con el stop objetivo
            var stops = new List<OrderGroupStop> { targetStop };
            dbMock.Setup(d => d.OrderGroupStops)
                  .Returns(MockDbSetHelper.CreateMockDbSet(stops).Object);

            // Mock de Drivers
            var drivers = driver != null
                ? new List<Driver> { driver }
                : new List<Driver>();
            dbMock.Setup(d => d.Drivers)
                  .Returns(MockDbSetHelper.CreateMockDbSet(drivers).Object);

            // Mock de AuditLogs (solo Add, no query)
            var auditLogs = new List<AuditLog>();
            dbMock.Setup(d => d.AuditLogs)
                  .Returns(MockDbSetHelper.CreateMockDbSet(auditLogs).Object);

            // SaveChangesAsync siempre exitoso
            dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

            var handler = new CompleteStopCommandHandler(dbMock.Object, notifMock.Object);
            return (handler, dbMock, notifMock);
        }

        // ════════════════════════════════════════════════════════════════════════
        // GRUPO 1: VALIDACIONES Y CASOS DE ERROR
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// TEST 1: Stop no encontrado → InvalidOperationException.
        /// Simula que el StopId no existe en la BD.
        /// </summary>
        [Fact]
        public async Task HandleAsync_StopNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var dbMock = new Mock<IApplicationDbContext>();
            var notifMock = new Mock<INotificationService>();

            // DbSet vacío — ningún stop existe
            dbMock.Setup(d => d.OrderGroupStops)
                  .Returns(MockDbSetHelper.CreateMockDbSet(new List<OrderGroupStop>()).Object);

            var handler = new CompleteStopCommandHandler(dbMock.Object, notifMock.Object);
            var command = new CompleteStopCommand(stopId: 999, driverId: DriverId);

            // Act
            var act = async () => await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*999*");
        }

        /// <summary>
        /// TEST 2: Driver no asignado al grupo → UnauthorizedAccessException.
        /// Seguridad: un driver no puede completar paradas de otro grupo.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WrongDriver_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var (_, pickupStop, _, _) = BuildSingleMerchantGroup(stopId: 1, StopType.Pickup);
            var (handler, _, _) = BuildHandler(pickupStop);

            // Comando con driver diferente al asignado
            var command = new CompleteStopCommand(stopId: 1, driverId: OtherDriverId);

            // Act
            var act = async () => await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage($"*{OtherDriverId}*");
        }

        /// <summary>
        /// TEST 3: SubOrder no encontrada para el MerchantId del Pickup → InvalidOperationException.
        /// Protege contra inconsistencias de datos entre Stops y SubOrders.
        /// </summary>
        [Fact]
        public async Task HandleAsync_PickupWithNoMatchingSubOrder_ThrowsInvalidOperationException()
        {
            // Arrange — Stop con MerchantId diferente al de la SubOrder
            var subOrder = new SubOrder { Id = 1, MerchantId = 999, Status = SubOrderStatus.PendingMerchantAcceptance };
            var pickupStop = new OrderGroupStop
            {
                Id = 1,
                StopType = StopType.Pickup,
                MerchantId = MerchantId, // MerchantId = 5, SubOrder tiene 999
                CompletedAt = null
            };
            var group = new OrderGroup
            {
                Id = GroupId,
                DriverId = DriverId,
                CustomerId = CustomerId,
                Status = OrderGroupStatus.DriverAccepted,
                SubOrders = new List<SubOrder> { subOrder },
                Stops = new List<OrderGroupStop> { pickupStop }
            };
            pickupStop.OrderGroup = group;

            var (handler, _, _) = BuildHandler(pickupStop);
            var command = new CompleteStopCommand(stopId: 1, driverId: DriverId);

            // Act
            var act = async () => await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{MerchantId}*");
        }

        // ════════════════════════════════════════════════════════════════════════
        // GRUPO 2: IDEMPOTENCIA
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// TEST 4: Stop ya completado → retorna resultado idempotente sin efectos secundarios.
        /// Protege contra doble tap en la app del driver o reintentos de red.
        /// </summary>
        [Fact]
        public async Task HandleAsync_StopAlreadyCompleted_ReturnsIdempotentResultWithoutSideEffects()
        {
            // Arrange — Stop con CompletedAt ya seteado
            var (group, pickupStop, _, _) = BuildSingleMerchantGroup(stopId: 1, StopType.Pickup);
            pickupStop.CompletedAt = DateTime.UtcNow.AddMinutes(-10); // Ya completado

            var (handler, dbMock, notifMock) = BuildHandler(pickupStop);
            var command = new CompleteStopCommand(stopId: 1, driverId: DriverId);

            // Act
            var result = await handler.HandleAsync(command, CancellationToken.None);

            // Assert — resultado válido
            result.StopId.Should().Be(1);
            result.Message.Should().Contain("ya fue completada");

            // Assert — sin efectos secundarios
            dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never,
                "No debe persistir nada si la parada ya estaba completada.");
            notifMock.Verify(n => n.NotifyDriverArrivedAtStopAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never,
                "No debe notificar si la parada ya estaba completada.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // GRUPO 3: PICKUP
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// TEST 5: Pickup completado → SubOrder pasa a PickedUp.
        /// Verifica la transición de estado de la SubOrder del comercio.
        /// </summary>
        [Fact]
        public async Task HandleAsync_PickupCompleted_SubOrderStatusIsPickedUp()
        {
            // Arrange
            var (_, pickupStop, _, subOrder) = BuildSingleMerchantGroup(stopId: 1, StopType.Pickup);
            var (handler, _, _) = BuildHandler(pickupStop);
            var command = new CompleteStopCommand(stopId: 1, driverId: DriverId);

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            subOrder.Status.Should().Be(SubOrderStatus.PickedUp,
                "el Pickup debe avanzar la SubOrder a PickedUp.");
            subOrder.PickedUpAt.Should().NotBeNull(
                "PickedUpAt debe registrarse al completar el Pickup.");
        }

        /// <summary>
        /// TEST 6: Último Pickup completado → OrderGroup pasa a InRoute.
        /// Cuando no quedan más Pickups pendientes, el driver va hacia el cliente.
        /// </summary>
        [Fact]
        public async Task HandleAsync_LastPickupCompleted_OrderGroupStatusIsInRoute()
        {
            // Arrange — solo 1 Pickup (el que vamos a completar)
            var (group, pickupStop, _, _) = BuildSingleMerchantGroup(stopId: 1, StopType.Pickup);
            var (handler, _, _) = BuildHandler(pickupStop);
            var command = new CompleteStopCommand(stopId: 1, driverId: DriverId);

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            group.Status.Should().Be(OrderGroupStatus.InRoute,
                "cuando se completa el último Pickup, el grupo debe pasar a InRoute.");
        }

        /// <summary>
        /// TEST 7: Pickup completado con otros Pickups pendientes → OrderGroup NO cambia a InRoute.
        /// Escenario multi-comercio: el driver aún debe visitar otros comercios.
        /// </summary>
        [Fact]
        public async Task HandleAsync_PickupCompletedWithPendingPickups_OrderGroupStatusUnchanged()
        {
            // Arrange — 2 Pickups: completamos el primero, el segundo sigue pendiente
            var subOrder1 = new SubOrder { Id = 1, MerchantId = MerchantId, Status = SubOrderStatus.PendingMerchantAcceptance };
            var subOrder2 = new SubOrder { Id = 2, MerchantId = MerchantId + 1, Status = SubOrderStatus.PendingMerchantAcceptance };

            var pickup1 = new OrderGroupStop { Id = 1, StopType = StopType.Pickup, MerchantId = MerchantId, CompletedAt = null };
            var pickup2 = new OrderGroupStop { Id = 2, StopType = StopType.Pickup, MerchantId = MerchantId + 1, CompletedAt = null }; // Pendiente
            var dropoff = new OrderGroupStop { Id = 3, StopType = StopType.Dropoff, CompletedAt = null };

            var group = new OrderGroup
            {
                Id = GroupId,
                DriverId = DriverId,
                CustomerId = CustomerId,
                Status = OrderGroupStatus.DriverAccepted,
                SubOrders = new List<SubOrder> { subOrder1, subOrder2 },
                Stops = new List<OrderGroupStop> { pickup1, pickup2, dropoff }
            };
            pickup1.OrderGroup = group;
            pickup2.OrderGroup = group;
            dropoff.OrderGroup = group;

            var (handler, _, _) = BuildHandler(pickup1);
            var command = new CompleteStopCommand(stopId: 1, driverId: DriverId);

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            group.Status.Should().Be(OrderGroupStatus.DriverAccepted,
                "aún hay Pickups pendientes, el grupo no debe pasar a InRoute.");
        }

        // ════════════════════════════════════════════════════════════════════════
        // GRUPO 4: DROPOFF
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// TEST 8: Dropoff completado → SubOrders en PickedUp pasan a Delivered.
        /// Verifica la transición de estado de todas las SubOrders del grupo.
        /// </summary>
        [Fact]
        public async Task HandleAsync_DropoffCompleted_SubOrdersAreDelivered()
        {
            // Arrange — SubOrder ya en PickedUp (driver ya recogió)
            var (_, _, dropoffStop, subOrder) = BuildSingleMerchantGroup(
                stopId: 1, StopType.Dropoff, SubOrderStatus.PickedUp);

            var (handler, _, _) = BuildHandler(dropoffStop);
            var command = new CompleteStopCommand(stopId: dropoffStop.Id, driverId: DriverId);

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            subOrder.Status.Should().Be(SubOrderStatus.Delivered,
                "el Dropoff debe marcar las SubOrders como Delivered.");
            subOrder.DeliveredAt.Should().NotBeNull(
                "DeliveredAt debe registrarse al completar el Dropoff.");
        }

        /// <summary>
        /// TEST 9: Dropoff final → OrderGroup pasa a Delivered y Driver queda liberado.
        /// Verifica el cierre completo del ciclo de entrega.
        /// </summary>
        [Fact]
        public async Task HandleAsync_FinalDropoffCompleted_OrderGroupDeliveredAndDriverReleased()
        {
            // Arrange
            var driver = new Driver
            {
                Id = DriverId,
                Status = DriverStatus.Busy,
                CurrentOrderGroupId = GroupId
            };

            var (group, _, dropoffStop, _) = BuildSingleMerchantGroup(
                stopId: 1, StopType.Dropoff, SubOrderStatus.PickedUp);

            var (handler, _, _) = BuildHandler(dropoffStop, driver);
            var command = new CompleteStopCommand(stopId: dropoffStop.Id, driverId: DriverId);

            // Act
            var result = await handler.HandleAsync(command, CancellationToken.None);

            // Assert — OrderGroup cerrado
            result.OrderGroupCompleted.Should().BeTrue();
            group.Status.Should().Be(OrderGroupStatus.Delivered);
            group.DeliveredAt.Should().NotBeNull();

            // Assert — Driver liberado
            driver.Status.Should().Be(DriverStatus.Available,
                "el driver debe quedar disponible tras completar la entrega.");
            driver.CurrentOrderGroupId.Should().BeNull(
                "CurrentOrderGroupId debe limpiarse para que el dispatch lo considere disponible.");
        }

        /// <summary>
        /// TEST 10: Dropoff con SubOrders aún en Pending (no PickedUp) → grupo NO se cierra.
        /// Escenario de error de datos o multi-Dropoff parcial.
        /// </summary>
        [Fact]
        public async Task HandleAsync_DropoffWithPendingSubOrders_OrderGroupNotCompleted()
        {
            // Arrange — SubOrder en Pending (no fue recogida aún)
            var (group, _, dropoffStop, subOrder) = BuildSingleMerchantGroup(
                stopId: 1, StopType.Dropoff, SubOrderStatus.PendingMerchantAcceptance); // No PickedUp

            var (handler, _, _) = BuildHandler(dropoffStop);
            var command = new CompleteStopCommand(stopId: dropoffStop.Id, driverId: DriverId);

            // Act
            var result = await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            result.OrderGroupCompleted.Should().BeFalse(
                "si hay SubOrders que no están en PickedUp, el grupo no puede cerrarse.");
            group.Status.Should().NotBe(OrderGroupStatus.Delivered);
        }

        // ════════════════════════════════════════════════════════════════════════
        // GRUPO 5: AUDITLOG Y PERSISTENCIA
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// TEST 11: Pickup completado → AuditLog registrado con datos correctos.
        /// Verifica que el log inmutable se inserta con la acción y el actor correcto.
        /// </summary>
        [Fact]
        public async Task HandleAsync_PickupCompleted_AuditLogIsAdded()
        {
            // Arrange
            var (_, pickupStop, _, _) = BuildSingleMerchantGroup(stopId: 1, StopType.Pickup);
            var (handler, dbMock, _) = BuildHandler(pickupStop);

            var capturedLogs = new List<AuditLog>();
            dbMock.Setup(d => d.AuditLogs.Add(It.IsAny<AuditLog>()))
                  .Callback<AuditLog>(log => capturedLogs.Add(log));

            var command = new CompleteStopCommand(stopId: 1, driverId: DriverId);

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            capturedLogs.Should().HaveCount(1, "debe registrarse exactamente 1 entrada en AuditLog.");
            capturedLogs[0].Action.Should().Contain("Pickup");
            capturedLogs[0].PerformedByUserId.Should().Be(DriverId);
            capturedLogs[0].EntityName.Should().Be(nameof(OrderGroupStop));
            capturedLogs[0].EntityId.Should().Be(1);
        }

        /// <summary>
        /// TEST 12: SaveChangesAsync se llama exactamente una vez por ejecución exitosa.
        /// Garantiza el patrón Unit of Work: un solo commit por operación.
        /// </summary>
        [Fact]
        public async Task HandleAsync_SuccessfulExecution_SaveChangesCalledOnce()
        {
            // Arrange
            var (_, pickupStop, _, _) = BuildSingleMerchantGroup(stopId: 1, StopType.Pickup);
            var (handler, dbMock, _) = BuildHandler(pickupStop);
            var command = new CompleteStopCommand(stopId: 1, driverId: DriverId);

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once,
                "SaveChangesAsync debe llamarse exactamente una vez (Unit of Work).");
        }

        // ════════════════════════════════════════════════════════════════════════
        // GRUPO 6: NOTIFICACIONES
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// TEST 13: Pickup completado → notifica al comercio (NotifyDriverArrivedAtStop).
        /// Verifica que el comercio recibe la notificación de llegada del driver.
        /// </summary>
        [Fact]
        public async Task HandleAsync_PickupCompleted_NotifiesMerchant()
        {
            // Arrange
            var (_, pickupStop, _, _) = BuildSingleMerchantGroup(stopId: 1, StopType.Pickup);
            var (handler, _, notifMock) = BuildHandler(pickupStop);
            var command = new CompleteStopCommand(stopId: 1, driverId: DriverId);

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            notifMock.Verify(n => n.NotifyDriverArrivedAtStopAsync(
                GroupId,
                1,
                "Pickup",
                MerchantId,
                It.IsAny<CancellationToken>()), Times.Once,
                "Debe notificar al comercio cuando el driver completa un Pickup.");
        }

        /// <summary>
        /// TEST 14: Dropoff final completado → notifica al cliente con estado "Delivered".
        /// Verifica que el cliente recibe la notificación de entrega exitosa.
        /// </summary>
        [Fact]
        public async Task HandleAsync_FinalDropoffCompleted_NotifiesCustomerWithDeliveredStatus()
        {
            // Arrange
            var driver = new Driver { Id = DriverId, Status = DriverStatus.Busy, CurrentOrderGroupId = GroupId };
            var (_, _, dropoffStop, _) = BuildSingleMerchantGroup(
                stopId: 1, StopType.Dropoff, SubOrderStatus.PickedUp);

            var (handler, _, notifMock) = BuildHandler(dropoffStop, driver);
            var command = new CompleteStopCommand(stopId: dropoffStop.Id, driverId: DriverId);

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            notifMock.Verify(n => n.NotifyDriverArrivedAtStopAsync(
                GroupId,
                dropoffStop.Id,
                "Delivered",
                CustomerId,
                It.IsAny<CancellationToken>()), Times.Once,
                "Debe notificar al cliente con estado 'Delivered' al completar el último Dropoff.");
        }

        /// <summary>
        /// TEST 15: Notas del driver se persisten en el Stop.
        /// Verifica que las notas opcionales del repartidor se guardan correctamente.
        /// </summary>
        [Fact]
        public async Task HandleAsync_WithNotes_NotesSavedOnStop()
        {
            // Arrange
            var (_, pickupStop, _, _) = BuildSingleMerchantGroup(stopId: 1, StopType.Pickup);
            var (handler, _, _) = BuildHandler(pickupStop);
            var command = new CompleteStopCommand(
                stopId: 1, driverId: DriverId, notes: "Cliente ausente, dejé con portero");

            // Act
            await handler.HandleAsync(command, CancellationToken.None);

            // Assert
            pickupStop.Notes.Should().Be("Cliente ausente, dejé con portero",
                "las notas del driver deben persistirse en el Stop.");
        }
    }
}

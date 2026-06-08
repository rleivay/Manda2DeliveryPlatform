// ═══════════════════════════════════════════════════════════════════════════
// SCHEMAS DEL PROYECTO — Manda2 Delivery Platform
//
//  core  → Entidades operativas principales del negocio.
//           Actores (Driver, Customer, Merchant), Órdenes base (Order,
//           SubOrder, SubOrderDetail), Catálogo (Product, MerchantProduct),
//           Direcciones (ShippingAddress) y Horarios (OperatingSchedule,
//           OperatingScheduleException).
//
//  ord   → Dominio de Órdenes Multi-local.
//           Agrupa las entidades del modelo de pedido grupal:
//           OrderGroup (ruta logística completa del cliente),
//           OrderGroupStop (paradas Pickup/Dropoff de la ruta).
//           Separado de core para facilitar permisos y evolución independiente.
//
//  dsp   → Motor de Dispatch y Logística.
//           Todo lo relacionado con la asignación de repartidores:
//           DispatchAttempt (rondas de búsqueda), DispatchAttemptDriver
//           (respuestas por driver), DispatchConfig (parámetros por zona),
//           DriverRejectionReason (catálogo de motivos de rechazo).
//
//  fin   → Dominio Financiero y Liquidaciones.
//           Movimientos de dinero entre actores de la plataforma:
//           CashDeposit (depósitos de efectivo del repartidor al operador),
//           CustomerCreditNote (notas de crédito emitidas a clientes).
//           Base para la integración futura con SAP B1 (liquidaciones,
//           documentos de pago, conciliación bancaria).
//
//  cfg   → Configuración y Catálogos Globales.
//           Parámetros del sistema administrables desde BackOffice sin
//           cambios en código: AppConfig (comisiones, fees, labels, dispatch),
//           PaymentMethod, ProductCategory, MerchantCategory,
//           CreditNoteType, ClosureReason.
//           Principio: ningún valor de negocio debe estar hardcodeado.
//
//  aud   → Auditoría y Trazabilidad.
//           Registro inmutable de acciones críticas del sistema: AuditLog.
//           Diseñado para cumplimiento regulatorio, soporte, forensics
//           y futura integración con SAP B1 (trazabilidad de documentos).
//           Las tablas de este schema son de solo escritura en operación
//           normal: nunca se actualizan, solo se insertan.
//
// ─── REGLA DE ORO ────────────────────────────────────────────────────────
//  Toda entidad nueva debe declarar su schema explícitamente con:
//  entity.ToTable("NombreTabla", "schema")
//  Nunca dejar que EF Core asigne dbo por defecto.
// ═══════════════════════════════════════════════════════════════════════════


using Manda2.Application.Common;
using Manda2.Domain.Common;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ═══════════════════════════════════════════════════════════════════════
        // DBSETS — Cada DbSet representa una tabla en SQL Server.
        // Se agrupan por dominio funcional para facilitar navegación.
        // ═══════════════════════════════════════════════════════════════════════

        // ─── ÓRDENES (Dominio Logístico Principal) ────────────────────────────
        // Order: entidad legada que puede coexistir durante migración a OrderGroup
        public DbSet<Order> Orders { get; set; }

        // OrderGroup: ruta logística completa de un cliente (multi-comercio)
        public DbSet<OrderGroup> OrderGroups { get; set; }

        // SubOrder: pedido individual por comercio dentro de un OrderGroup
        public DbSet<SubOrder> SubOrders { get; set; }

        // SubOrderDetail: línea de producto dentro de una SubOrder (snapshot histórico)
        public DbSet<SubOrderDetail> SubOrderDetails { get; set; }

        // OrderGroupStop: paradas de la ruta del repartidor (Pickup + Dropoff)
        public DbSet<OrderGroupStop> OrderGroupStops { get; set; }

        // ─── DISPATCH (Motor de Asignación de Repartidores) ───────────────────
        // DispatchAttempt: cada ronda de búsqueda de repartidor para un OrderGroup
        public DbSet<DispatchAttempt> DispatchAttempts { get; set; }

        // DispatchAttemptDriver: respuesta individual de cada driver notificado
        public DbSet<DispatchAttemptDriver> DispatchAttemptDrivers { get; set; }

        // DispatchConfig: parámetros operativos configurables por Zona/Ciudad
        public DbSet<DispatchConfig> DispatchConfigs { get; set; }

        // ─── ACTORES ──────────────────────────────────────────────────────────
        // Driver: repartidor con capacidad, posición GPS y estado operativo
        public DbSet<Driver> Drivers { get; set; }

        // Catálogo de motivos de rechazo de ofertas (repartidor)
        public DbSet<DriverRejectionReason> DriverRejectionReasons { get; set; }

        // Customer: cliente de la plataforma
        public DbSet<Customer> Customers { get; set; }

        // Merchant: comercio afiliado (Marketplace, DarkKitchen, DarkStore)
        public DbSet<Merchant> Merchants { get; set; }

        // ─── CATÁLOGO ─────────────────────────────────────────────────────────
        public DbSet<Product> Products { get; set; }
        public DbSet<MerchantProduct> MerchantProducts { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<MerchantCategory> MerchantCategories { get; set; }

        // ─── FINANCIERO ───────────────────────────────────────────────────────
        // CashDeposit: depósitos de efectivo del repartidor al operador
        public DbSet<CashDeposit> CashDeposits { get; set; }

        // CreditNote: notas de crédito emitidas a clientes
        public DbSet<CreditNoteType> CreditNoteTypes { get; set; }
        public DbSet<CustomerCreditNote> CustomerCreditNotes { get; set; }

        // ─── PAGOS ────────────────────────────────────────────────────────────
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<OrderGroupPayment> OrderGroupPayments { get; set; }

        // ─── DIRECCIÓN ────────────────────────────────────────────────────────
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }

        // ─── HORARIOS Y CIERRES ───────────────────────────────────────────────
        // OperatingSchedule: horario base semanal de Merchant o Driver
        public DbSet<OperatingSchedule> OperatingSchedules { get; set; }

        // OperatingScheduleException: cierre o apertura especial por fecha
        public DbSet<OperatingScheduleException> OperatingScheduleExceptions { get; set; }

        // ClosureReason: catálogo de razones de cierre (configurable desde BackOffice)
        public DbSet<ClosureReason> ClosureReasons { get; set; }

        // ─── CONFIGURACIÓN Y AUDITORÍA ────────────────────────────────────────
        // AppConfig: parámetros globales del sistema (comisiones, fees, labels, etc.)
        public DbSet<AppConfig> AppConfigs { get; set; }

        // AuditLog: registro de acciones críticas del sistema
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }

        // ═══════════════════════════════════════════════════════════════════════
        // ON MODEL CREATING
        // Aquí se define la estructura física de la base de datos:
        // - Schemas SQL por dominio
        // - Tipos de columna (decimal, varchar, etc.)
        // - Relaciones y comportamiento de borrado
        // - Índices para performance
        // - Check Constraints para integridad
        // - Seed Data inicial
        // ═══════════════════════════════════════════════════════════════════════
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ───────────────────────────────────────────────────────────────────
            // PASO 0: PRECISIÓN DECIMAL GLOBAL
            // Por defecto, todos los campos decimal usan decimal(18,4)
            // compatible con SAP B1 y suficiente para monedas locales.
            // EXCEPCIÓN: GPS usa decimal(18,10) — se sobreescribe más abajo.
            // ───────────────────────────────────────────────────────────────────
            foreach (var property in modelBuilder.Model
                         .GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,4)");
            }

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 1: SCHEMAS SQL POR DOMINIO
            // Organizamos las tablas en schemas para separar responsabilidades:
            // - core: entidades operativas principales
            // - fin:  entidades financieras y de liquidación
            // - cfg:  configuración y catálogos
            // - aud:  auditoría y trazabilidad
            // - dsp:  dispatch y logística (NUEVO Fase D2)
            // ═══════════════════════════════════════════════════════════════════

            // ─── SCHEMA: core ─────────────────────────────────────────────────
            modelBuilder.Entity<Driver>(b => b.ToTable("Drivers", schema: "core"));
            modelBuilder.Entity<Customer>(b => b.ToTable("Customers", schema: "core"));
            modelBuilder.Entity<Merchant>(b => b.ToTable("Merchants", schema: "core"));
            modelBuilder.Entity<Order>(b => b.ToTable("Orders", schema: "core"));
            modelBuilder.Entity<SubOrder>(b => b.ToTable("SubOrders", schema: "core"));
            modelBuilder.Entity<SubOrderDetail>(b => b.ToTable("SubOrderDetails", schema: "core"));
            modelBuilder.Entity<ShippingAddress>(b => b.ToTable("ShippingAddresses", schema: "core"));
            modelBuilder.Entity<OperatingSchedule>(b => b.ToTable("OperatingSchedules", schema: "core"));
            modelBuilder.Entity<OperatingScheduleException>(b => b.ToTable("OperatingScheduleExceptions", schema: "core"));
            modelBuilder.Entity<Product>(b => b.ToTable("Products", schema: "core"));
            modelBuilder.Entity<MerchantProduct>(b => b.ToTable("MerchantProducts", schema: "core"));

            // ─── SCHEMA: ord (Nuevo — Órdenes Multi-local) ───────────────────
            // Separamos OrderGroup y sus entidades relacionadas en schema propio
            // para facilitar permisos y mantenimiento futuro.
            modelBuilder.Entity<OrderGroup>(b => b.ToTable("OrderGroups", schema: "ord"));
            modelBuilder.Entity<OrderGroupStop>(b => b.ToTable("OrderGroupStops", schema: "ord"));

            // ─── SCHEMA: dsp (Nuevo — Dispatch Engine) ────────────────────────
            modelBuilder.Entity<DispatchAttempt>(b => b.ToTable("DispatchAttempts", schema: "dsp"));
            modelBuilder.Entity<DispatchAttemptDriver>(b => b.ToTable("DispatchAttemptDrivers", schema: "dsp"));
            modelBuilder.Entity<DispatchConfig>(b => b.ToTable("DispatchConfigs", schema: "dsp"));

            // ─── SCHEMA: fin ──────────────────────────────────────────────────
            modelBuilder.Entity<CashDeposit>(b => b.ToTable("CashDeposits", schema: "fin"));
            modelBuilder.Entity<CustomerCreditNote>(b => b.ToTable("CustomerCreditNotes", schema: "fin"));
            modelBuilder.Entity<OrderGroupPayment>(entity =>
            {
                entity.ToTable("OrderGroupPayments", "fin");

                // Relación OrderGroup -> Payments (Cascada: si se borra el grupo en Draft, se borran sus intentos de pago)
                entity.HasOne(d => d.OrderGroup)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.OrderGroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación PaymentMethod (Restrict: no borrar el catálogo si hay transacciones)
                entity.HasOne(d => d.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(d => d.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Total).HasPrecision(18, 4);
                entity.Property(e => e.Reference).HasMaxLength(100);
                entity.Property(e => e.Authorization).HasMaxLength(100);
                entity.Property(e => e.ProofUrl).HasMaxLength(500);

                // Índice para búsquedas rápidas por referencia (BackOffice validation)
                entity.HasIndex(e => e.Reference)
                    .HasDatabaseName("IX_OrderGroupPayment_Reference");
            });
            // ─── SCHEMA: cfg ──────────────────────────────────────────────────
            modelBuilder.Entity<AppConfig>(b => b.ToTable("AppConfigs", schema: "cfg"));
            modelBuilder.Entity<ProductCategory>(b => b.ToTable("ProductCategories", schema: "cfg"));
            modelBuilder.Entity<MerchantCategory>(b => b.ToTable("MerchantCategories", schema: "cfg"));
            modelBuilder.Entity<PaymentMethod>(b => b.ToTable("PaymentMethods", schema: "cfg"));
            modelBuilder.Entity<CreditNoteType>(b => b.ToTable("CreditNoteTypes", schema: "cfg"));
            modelBuilder.Entity<ClosureReason>(b => b.ToTable("ClosureReasons", schema: "cfg"));

            // ─── SCHEMA: aud ──────────────────────────────────────────────────
            modelBuilder.Entity<AuditLog>(b => b.ToTable("AuditLogs", schema: "aud"));


            // ─── SCHEMA: sec ────
            modelBuilder.Entity<AppUser>(entity =>
                {
                    entity.ToTable("AppUsers", "sec");
                    entity.HasIndex(u => u.Email).IsUnique();
                    entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
                    entity.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
                    entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
                    entity.Property(u => u.RefreshToken).HasMaxLength(200);

                    // Check Constraint: solo un actor vinculado por usuario
                    entity.ToTable(t => t.HasCheckConstraint(
                        "CK_AppUser_SingleActor",
                        @"(CASE WHEN CustomerId IS NOT NULL THEN 1 ELSE 0 END +
                           CASE WHEN DriverId   IS NOT NULL THEN 1 ELSE 0 END +
                           CASE WHEN MerchantId IS NOT NULL THEN 1 ELSE 0 END) <= 1"
                    ));
                });

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 2: CONFIGURACIÓN DE ENTIDADES — CATÁLOGO
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<Merchant>(entity =>
            {
                // Comisión del comercio con precisión extendida para cascada
                entity.Property(x => x.CommissionPct).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DefaultPreparationMinutes).HasDefaultValue(15);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                // Relación Producto → Categoría (no se puede borrar categoría con productos)
                entity.HasOne(x => x.Category)
                      .WithMany()
                      .HasForeignKey(x => x.ProductCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                // Comisión por categoría (nivel 2 de la cascada de comisiones)
                entity.Property(x => x.CommissionPct).HasColumnType("decimal(18,4)");
            });

            modelBuilder.Entity<MerchantProduct>(entity =>
            {
                // Precios con precisión SAP B1
                entity.Property(x => x.BasePrice).HasColumnType("decimal(18,4)");
                entity.Property(x => x.SalePrice).HasColumnType("decimal(18,4)");
                entity.Property(x => x.CommissionPctOverride).HasColumnType("decimal(18,4)");
                entity.Property(x => x.ResolvedCommissionPct).HasColumnType("decimal(18,4)");

                // Enums guardados como int en SQL (más eficiente que string)
                entity.Property(x => x.CommissionSource).HasConversion<int>();
                entity.Property(x => x.ResolvedCommissionSource).HasConversion<int>();

                // Un producto no puede estar duplicado en el mismo comercio
                entity.HasIndex(x => new { x.MerchantId, x.ProductId }).IsUnique();

                entity.HasOne(x => x.Merchant)
                      .WithMany()
                      .HasForeignKey(x => x.MerchantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 3: CONFIGURACIÓN DE ENTIDADES — ÓRDENES MULTI-LOCAL (NUEVO)
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<OrderGroup>(entity =>
            {
                // Estado del grupo guardado como int (Enum OrderGroupStatus)
                entity.Property(og => og.Status)
                      .HasConversion<int>();
                //.HasDefaultValue(OrderGroupStatus.Draft)

                // Financiero del grupo consolidado
                entity.Property(og => og.DeliveryFee).HasColumnType("decimal(18,2)");
                entity.Property(og => og.ServiceFee).HasColumnType("decimal(18,2)");
                entity.Property(og => og.TotalAmount).HasColumnType("decimal(18,2)");

                // GPS con precisión milimétrica decimal(18,10)
                // Sobreescribe el decimal(18,4) global para estos campos
                entity.Property(og => og.DeliveryLatitude).HasColumnType("decimal(18,10)");
                entity.Property(og => og.DeliveryLongitude).HasColumnType("decimal(18,10)");

                entity.Property(og => og.DeliveryAddressText).HasMaxLength(500);

                // Cliente: no se puede borrar si tiene pedidos históricos
                entity.HasOne(og => og.Customer)
                      .WithMany()
                      .HasForeignKey(og => og.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Driver: nullable (aún no asignado al crear el grupo)
                // SetNull: si se elimina el driver, el grupo queda sin asignar
                entity.HasOne(og => og.Driver)
                      .WithMany()
                      .HasForeignKey(og => og.DriverId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            modelBuilder.Entity<SubOrder>(entity =>
            {
                // Estado de la suborden por comercio
                entity.Property(s => s.Status)
                      .HasConversion<int>();
                //.HasDefaultValue(SubOrderStatus.PendingMerchantAcceptance);

                // Financiero por comercio
                entity.Property(s => s.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(s => s.ResolvedCommissionPct).HasColumnType("decimal(5,2)");
                entity.Property(s => s.TotalCommissionAmount).HasColumnType("decimal(18,2)");
                entity.Property(s => s.DeliveryFeeProrrated).HasColumnType("decimal(18,2)");
                entity.Property(s => s.NetPayable).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DeliveryDistanceKm).HasColumnType("decimal(10,4)").IsRequired(false);
                entity.Property(e => e.DeliveryDistancePct).HasColumnType("decimal(8,4)").IsRequired(false);
                entity.Property(e => e.SnapshotFeeBase).HasColumnType("decimal(18,4)").IsRequired(false);
                entity.Property(e => e.SnapshotFeeKmIncluidos).HasColumnType("decimal(10,4)").IsRequired(false);
                entity.Property(e => e.SnapshotFeePorKm).HasColumnType("decimal(18,4)").IsRequired(false);
                entity.Property(e => e.SnapshotTotalGroupDistanceKm).HasColumnType("decimal(10,4)").IsRequired(false);
                entity.Property(e => e.SnapshotTotalGroupFee).HasColumnType("decimal(18,4)").IsRequired(false);

                // Cascade: si se borra el OrderGroup, se borran sus SubOrders
                entity.HasOne(s => s.OrderGroup)
                      .WithMany(og => og.SubOrders)
                      .HasForeignKey(s => s.OrderGroupId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Restrict: no se puede borrar un comercio con historial de pedidos
                entity.HasOne(s => s.Merchant)
                      .WithMany()
                      .HasForeignKey(s => s.MerchantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SubOrderDetail>(entity =>
            {
                // Snapshot histórico: longitudes para nombres e imágenes
                entity.Property(d => d.ItemName).HasMaxLength(200);
                entity.Property(d => d.SnapshotImageUrl).HasMaxLength(500);
                entity.Property(d => d.Notes).HasMaxLength(300);
                entity.Property(d => d.SAP_ItemCode).HasMaxLength(50);

                // Precios snapshot al momento de la compra
                entity.Property(d => d.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.Property(d => d.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(d => d.UnitDiscount).HasColumnType("decimal(18,2)");
                entity.Property(d => d.NetSalePrice).HasColumnType("decimal(18,2)");
                entity.Property(d => d.CommissionPct).HasColumnType("decimal(5,2)");
                entity.Property(d => d.TaxPct).HasColumnType("decimal(5,2)");
                entity.Property(d => d.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(d => d.LineTotal).HasColumnType("decimal(18,2)");
                entity.Property(d => d.LineTotalWithTax).HasColumnType("decimal(18,2)");

                // Cascade: si se borra la SubOrder, se borran sus líneas de detalle
                entity.HasOne(d => d.SubOrder)
                      .WithMany(s => s.Details)
                      .HasForeignKey(d => d.SubOrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Restrict: no borrar producto si tiene historial de ventas
                entity.HasOne(d => d.MerchantProduct)
                      .WithMany()
                      .HasForeignKey(d => d.MerchantProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderGroupStop>(entity =>
            {
                // Tipo de parada: Pickup (recoger en comercio) o Dropoff (entregar al cliente)
                entity.Property(s => s.StopType).HasConversion<int>();

                entity.Property(s => s.AddressText).HasMaxLength(500);
                entity.Property(s => s.Notes).HasMaxLength(300);

                // GPS con precisión milimétrica
                entity.Property(s => s.Latitude).HasColumnType("decimal(18,10)");
                entity.Property(s => s.Longitude).HasColumnType("decimal(18,10)");

                // Cascade: si se borra el grupo, se borran sus paradas
                entity.HasOne(s => s.OrderGroup)
                      .WithMany(og => og.Stops)
                      .HasForeignKey(s => s.OrderGroupId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Nullable: el Dropoff no tiene comercio asociado
                entity.HasOne(s => s.Merchant)
                      .WithMany()
                      .HasForeignKey(s => s.MerchantId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                // Índice compuesto para ordenar paradas eficientemente en la ruta
                entity.HasIndex(s => new { s.OrderGroupId, s.Sequence });
            });

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 4: CONFIGURACIÓN DE ENTIDADES — DISPATCH ENGINE (NUEVO)
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<DispatchAttempt>(entity =>
            {
                // Estado de la ronda de dispatch
                entity.Property(da => da.Status).HasConversion<int>();

                // Cascade: si se borra el OrderGroup, se borra su historial de dispatch
                entity.HasOne(da => da.OrderGroup)
                      .WithMany(og => og.DispatchAttempts)
                      .HasForeignKey(da => da.OrderGroupId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Strategy).HasConversion<string>(); // Guardar enum como string

                // Índice para auditoría: consultas por pedido y número de ronda
                entity.HasIndex(da => new { da.OrderGroupId, da.RoundNumber });
            });

            modelBuilder.Entity<DispatchAttemptDriver>(entity =>
            {
                // Respuesta del driver: Pending, Accepted, Rejected, Expired
                entity.Property(dad => dad.Response)
                      .HasConversion<int>();
                //.HasDefaultValue(DriverDispatchResponse.Pending);

                // Cascade: si se borra el intento, se borran las respuestas
                entity.HasOne(dad => dad.DispatchAttempt)
                      .WithMany(da => da.NotifiedDrivers)
                      .HasForeignKey(dad => dad.DispatchAttemptId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Restrict: no borrar driver si tiene historial de dispatch
                entity.HasOne(dad => dad.Driver)
                      .WithMany()
                      .HasForeignKey(dad => dad.DriverId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Un driver no puede aparecer dos veces en el mismo intento de dispatch
                entity.HasIndex(dad => new { dad.DispatchAttemptId, dad.DriverId })
                      .IsUnique();
            });

            modelBuilder.Entity<DispatchConfig>(entity =>
            {
                entity.Property(dc => dc.ZoneName).HasMaxLength(100).IsRequired();

                // Radio inicial y factor de expansión por ronda
                // Ejemplo: Ronda 1 = 3km, Ronda 2 = 3+(1×2)=5km, Ronda 3 = 3+(2×2)=7km
                entity.Property(dc => dc.InitialRadiusKm).HasColumnType("decimal(5,2)");
                entity.Property(dc => dc.RadiusExpansionFactor).HasColumnType("decimal(5,2)");

                // Una sola configuración activa por zona/ciudad
                entity.HasIndex(dc => dc.ZoneName).IsUnique();
                entity.Property(e => e.FairnessWeight).HasPrecision(5, 2);
                entity.Property(e => e.DistanceWeight).HasPrecision(5, 2);
                entity.Property(e => e.SlaWeight).HasPrecision(5, 2);
                entity.Property(e => e.ReliabilityWeight).HasPrecision(5, 2);

            });

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 5: CONFIGURACIÓN DE ENTIDADES — DRIVER (ACTUALIZADO)
            // Nuevos campos de capacidad y GPS para el motor de dispatch
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<Driver>(entity =>
            {
                // GPS del driver con precisión milimétrica
                entity.Property(d => d.LastLatitude).HasColumnType("decimal(18,10)");
                entity.Property(d => d.LastLongitude).HasColumnType("decimal(18,10)");

                // FK al OrderGroup activo:
                // - NULL = driver libre, disponible para recibir pedidos
                // - Con valor = driver ocupado, no elegible para dispatch
                // SetNull: si el OrderGroup se elimina, el driver queda libre automáticamente
                entity.HasOne<OrderGroup>()
                      .WithMany()
                      .HasForeignKey(d => d.CurrentOrderGroupId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 6: CONFIGURACIÓN DE ENTIDADES — ACTORES Y FINANCIERO
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<CashDeposit>()
                .HasOne(c => c.Driver)
                .WithMany(d => d.CashDeposits)
                .HasForeignKey(c => c.DriverId);

            // ShippingAddress pertenece a un Customer
            // Restrict: no borrar cliente si tiene direcciones guardadas
            modelBuilder.Entity<ShippingAddress>(entity =>
            {
                entity.HasOne(x => x.Customer)
          .WithMany(x => x.ShippingAddresses)
          .HasForeignKey(x => x.CustomerId)
          .OnDelete(DeleteBehavior.Restrict);

            });

            // CustomerCreditNote pertenece a un Customer
            modelBuilder.Entity<CustomerCreditNote>(entity =>
            {
                entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,4)");
                entity.Property(x => x.RemainingAmount).HasColumnType("decimal(18,4)");

                entity.HasOne<Customer>()
                      .WithMany()
                      .HasForeignKey(x => x.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 7: HORARIOS Y CIERRES
            // Diseño polimórfico: un horario puede pertenecer a Merchant O Driver
            // El Check Constraint garantiza que nunca pertenezca a ambos o a ninguno
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<OperatingSchedule>(entity =>
            {
                // Garantiza integridad: exactamente uno de los dos IDs debe estar lleno
                entity.ToTable(t => t.HasCheckConstraint("CK_OperatingSchedule_Actor",
                    "(MerchantId IS NOT NULL AND DriverId IS NULL) OR (MerchantId IS NULL AND DriverId IS NOT NULL)"));

                entity.HasOne(d => d.Merchant)
                      .WithMany(p => p.OperatingSchedules)
                      .HasForeignKey(d => d.MerchantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Driver)
                      .WithMany(p => p.OperatingSchedules)
                      .HasForeignKey(d => d.DriverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ClosureReason)
                      .WithMany()
                      .HasForeignKey(d => d.ClosureReasonId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OperatingScheduleException>(entity =>
            {
                // Mismo patrón polimórfico que OperatingSchedule
                entity.ToTable(t => t.HasCheckConstraint("CK_OperatingScheduleException_Actor",
                    "(MerchantId IS NOT NULL AND DriverId IS NULL) OR (MerchantId IS NULL AND DriverId IS NOT NULL)"));

                entity.HasOne(d => d.Merchant)
                      .WithMany(p => p.OperatingScheduleExceptions)
                      .HasForeignKey(d => d.MerchantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Driver)
                      .WithMany(p => p.OperatingScheduleExceptions)
                      .HasForeignKey(d => d.DriverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ClosureReason)
                      .WithMany()
                      .HasForeignKey(d => d.ClosureReasonId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClosureReason>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // ─── DriverRejectionReason ────────────────────────────────────────────────
            modelBuilder.Entity<DriverRejectionReason>(entity =>
            {
                entity.ToTable("DriverRejectionReasons", "dsp");

                // Code debe ser único: evita duplicados como dos "OTHER"
                entity.HasIndex(e => e.Code).IsUnique();

                entity.Property(e => e.Code)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.DisplayName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(e => e.RequiresNote)
                      .HasDefaultValue(false);

                entity.Property(e => e.DisplayOrder)
                      .HasDefaultValue(0);

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                // Relación inversa: un motivo puede aparecer en muchos rechazos históricos
                entity.HasMany(e => e.RejectionHistory)
                      .WithOne(d => d.RejectionReason)
                      .HasForeignKey(d => d.ReasonId)
                      .OnDelete(DeleteBehavior.Restrict); // No borrar motivos con historial
            });

            // ─── DispatchAttemptDriver — campos nuevos v2 ─────────────────────────────
            modelBuilder.Entity<DispatchAttemptDriver>(entity =>
            {
                // ReasonNotes: máximo 500 caracteres, nullable
                entity.Property(e => e.ReasonNotes)
                      .HasMaxLength(500);

                // ReasonId es nullable: solo se popula en rechazos
                entity.Property(e => e.ReasonId)
                      .IsRequired(false);
            });

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 8: SEED DATA — VALORES INICIALES DE CONFIGURACIÓN
            // Estos valores se insertan automáticamente al correr la migración.
            // Son configurables desde BackOffice sin necesidad de reprogramar.
            // ═══════════════════════════════════════════════════════════════════

            modelBuilder.Entity<AppConfig>().HasData(
                // ─── DRIVER ───────────────────────────────────────────────────
                new AppConfig
                {
                    Id = 1,
                    Key = "DEFAULT_DRIVER_MAX_CASH_LIMIT",
                    Value = "2000.00",
                    Description = "Límite de efectivo por defecto para nuevos repartidores"
                },
                new AppConfig
                {
                    Id = 2,
                    Key = "DEFAULT_DRIVER_MAX_SUBORDER_LIMIT",
                    // Renombrado de MAX_ACTIVE_GROUPS → MAX_SUBORDER_LIMIT (decisión D11)
                    Value = "1",
                    Description = "Cantidad máxima de SubOrders (comercios) que puede llevar un repartidor por defecto"
                },

                // ─── COMISIONES (Configurables por tipo de comercio) ──────────
                new AppConfig
                {
                    Id = 3,
                    Key = "COMMISSION_MARKETPLACE",
                    Value = "18.00",
                    Description = "% de comisión para comercios tipo Marketplace"
                },
                new AppConfig
                {
                    Id = 4,
                    Key = "COMMISSION_DARKSTORE",
                    Value = "25.00",
                    Description = "% de comisión para comercios tipo DarkStore"
                },
                new AppConfig
                {
                    Id = 5,
                    Key = "COMMISSION_DARKKITCHEN",
                    Value = "0.00",
                    Description = "% de comisión para comercios tipo DarkKitchen (pendiente de definir)"
                },

                // ─── FEES ─────────────────────────────────────────────────────
                new AppConfig
                {
                    Id = 6,
                    Key = "SERVICE_FEE_FIXED",
                    Value = "50.00",
                    Description = "Cargo fijo por servicio de plataforma"
                },
                new AppConfig
                {
                    Id = 7,
                    Key = "DELIVERY_FEE_BASE",
                    Value = "80.00",
                    Description = "Cargo base por delivery (puede variar por zona)"
                },

                // ─── LABELS CONFIGURABLES ─────────────────────────────────────
                new AppConfig
                {
                    Id = 8,
                    Key = "LABEL_SAME_PRICE_AS_STORE",
                    Value = "Mismo precio que en el local",
                    Description = "Etiqueta configurable para indicar precio igual al local físico"
                },

                // ─── DISPATCH ─────────────────────────────────────────────────
                new AppConfig
                {
                    Id = 9,
                    Key = "MAX_DISTANCE_DEVIATION_MTS",
                    Value = "2000",
                    Description = "Desviación máxima permitida en metros para agrupación de rutas"
                },
                new AppConfig
                {
                    Id = 10,
                    Key = "DISPATCH_DEFAULT_INITIAL_RADIUS_KM",
                    Value = "3.00",
                    Description = "Radio inicial de búsqueda de repartidores en la Ronda 1"
                },
                new AppConfig
                {
                    Id = 11,
                    Key = "DISPATCH_DEFAULT_RADIUS_EXPANSION_FACTOR",
                    Value = "2.00",
                    Description = "Kilómetros adicionales por cada ronda extra de dispatch"
                },
                new AppConfig
                {
                    Id = 12,
                    Key = "DISPATCH_DEFAULT_MAX_ROUNDS",
                    Value = "3",
                    Description = "Número máximo de rondas de búsqueda antes de notificar al cliente"
                },
                new AppConfig
                {
                    Id = 13,
                    Key = "DISPATCH_DEFAULT_ROUND_TIMEOUT_MINUTES",
                    Value = "2",
                    Description = "Minutos de espera por ronda antes de pasar a la siguiente"
                },
                new AppConfig
                {
                    Id = 14,
                    Key = "DISPATCH_DEFAULT_MAX_DRIVERS_PER_ROUND",
                    Value = "5",
                    Description = "Cantidad de repartidores notificados simultáneamente por ronda"
                },

                // ─── TRACKING Y PRESENCIA ───────────────────────────────────────────────────
                new AppConfig
                {
                    Id = 15,
                    Key = "DRIVER_GPS_UPDATE_INTERVAL_SECONDS",
                    Value = "30",
                    Description = "Frecuencia con la que la App del Driver debe reportar GPS (al estar en ruta)"
                },
                new AppConfig
                {
                    Id = 16,
                    Key = "DRIVER_GPS_MIN_DISTANCE_METERS",
                    Value = "50",
                    Description = "Distancia mínima recorrida para forzar una actualización de GPS antes del intervalo"
                },
                new AppConfig
                {
                    Id = 17,
                    Key = "DRIVER_STALE_LOCATION_MINUTES",
                    Value = "10",
                    Description = "Minutos tras los cuales una ubicación se considera obsoleta para el motor de Dispatch"
                }
                            );

            // ─── Seed: Motivos de rechazo iniciales ──────────────────────────────────
            modelBuilder.Entity<DriverRejectionReason>().HasData(
                new DriverRejectionReason { Id = 1, Code = "TOO_FAR", DisplayName = "Muy lejos", RequiresNote = false, DisplayOrder = 1, IsActive = true },
                new DriverRejectionReason { Id = 2, Code = "LOW_EARNINGS", DisplayName = "Ganancia baja", RequiresNote = false, DisplayOrder = 2, IsActive = true },
                new DriverRejectionReason { Id = 3, Code = "UNSAFE_ZONE", DisplayName = "Zona insegura", RequiresNote = false, DisplayOrder = 3, IsActive = true },
                new DriverRejectionReason { Id = 4, Code = "NO_BATTERY", DisplayName = "Sin batería", RequiresNote = false, DisplayOrder = 4, IsActive = true },
                new DriverRejectionReason { Id = 5, Code = "OTHER", DisplayName = "Otro motivo", RequiresNote = true, DisplayOrder = 5, IsActive = true }
            );

            // ─── Seed: PaymentMethod ──────────────────────────────────────────────────
            // Métodos de pago iniciales de la plataforma.
            // RequiresVoucher: el operador debe validar comprobante (Transferencia).
            // RequiresReference: el sistema debe guardar número de referencia/autorización.
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod
                {
                    Id = 1,
                    Name = "Efectivo",
                    Code = "CASH",
                    RequiresVoucher = false,
                    RequiresReference = false,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new PaymentMethod
                {
                    Id = 2,
                    Name = "Tarjeta",
                    Code = "CARD",
                    RequiresVoucher = false,   // La pasarela confirma automáticamente
                    RequiresReference = true,  // Guardar token/autorización de la pasarela
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new PaymentMethod
                {
                    Id = 3,
                    Name = "Transferencia",
                    Code = "TRANSFER",
                    RequiresVoucher = true,    // BackOffice debe validar comprobante
                    RequiresReference = true,  // Número de transferencia bancaria
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new PaymentMethod
                {
                    Id = 4,
                    Name = "Nota de Crédito",
                    Code = "CREDIT_NOTE",
                    RequiresVoucher = false,
                    RequiresReference = true,  // ID de la nota de crédito aplicada
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ─── Seed: CreditNoteType ─────────────────────────────────────────────────
            // Tipos de nota de crédito emitibles a clientes.
            // Extensible desde BackOffice sin cambios en código.
            modelBuilder.Entity<CreditNoteType>().HasData(
                new CreditNoteType
                {
                    Id = 1,
                    Name = "Devolución por pedido cancelado",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new CreditNoteType
                {
                    Id = 2,
                    Name = "Compensación por error de entrega",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new CreditNoteType
                {
                    Id = 3,
                    Name = "GiftCard / Promoción",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new CreditNoteType
                {
                    Id = 4,
                    Name = "Ajuste manual BackOffice",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ─── Seed: ClosureReason ──────────────────────────────────────────────────
            // Razones de cierre para Merchant y Driver (polimórfico).
            // Usadas en OperatingScheduleException para justificar cierres especiales.
            modelBuilder.Entity<ClosureReason>().HasData(
                new ClosureReason
                {
                    Id = 1,
                    Name = "Día feriado",
                    Description = "Cierre por día feriado nacional o local",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClosureReason
                {
                    Id = 2,
                    Name = "Mantenimiento",
                    Description = "Cierre temporal por mantenimiento del local o vehículo",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClosureReason
                {
                    Id = 3,
                    Name = "Falta de insumos",
                    Description = "Comercio sin stock suficiente para operar",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClosureReason
                {
                    Id = 4,
                    Name = "Emergencia",
                    Description = "Cierre de emergencia no planificado",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClosureReason
                {
                    Id = 5,
                    Name = "Vacaciones",
                    Description = "Cierre programado por período vacacional",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClosureReason
                {
                    Id = 6,
                    Name = "Otro",
                    Description = "Razón no categorizada — requiere nota manual",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ─── Seed: DispatchConfig ─────────────────────────────────────────────────
            // Configuración inicial de dispatch para la zona principal.
            // Todos los valores son sobreescribibles desde BackOffice por zona.
            // Algoritmo de radio: Ronda N = InitialRadiusKm + ((N-1) × RadiusExpansionFactor)
            // Ejemplo: R1=3km, R2=5km, R3=7km
            modelBuilder.Entity<DispatchConfig>().HasData(
                new DispatchConfig
                {
                    Id = 1,
                    ZoneName = "Nicaragua",
                    InitialRadiusKm = 3.00m,
                    RadiusExpansionFactor = 2.00m,
                    MaxDriversToNotifyPerRound = 5,
                    RoundTimeoutMinutes = 2,
                    MaxRounds = 3,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ═══════════════════════════════════════════════════════════════════
            // SECCIÓN 9: GLOBAL QUERY FILTERS — SOFT DELETE
            // Todas las entidades que hereden de BaseEntity serán filtradas
            // automáticamente para excluir registros con IsDeleted = true.
            // Esto significa que ninguna query retornará registros "borrados"
            // a menos que se use .IgnoreQueryFilters() explícitamente.
            // ═══════════════════════════════════════════════════════════════════
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
            {
                // Construcción dinámica del filtro: e => !e.IsDeleted
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var prop = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var compare = Expression.Equal(prop, Expression.Constant(false));
                var lambda = Expression.Lambda(compare, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SAVECHANGES ASYNC — LÓGICA CENTRALIZADA
        // Intercepta todas las operaciones de escritura para:
        // 1. Asignar CreatedAt automáticamente en inserts
        // 2. Asignar UpdatedAt automáticamente en updates
        // 3. Convertir Delete físico en Soft Delete (IsDeleted = true)
        // Esto garantiza que NUNCA se borre un registro físicamente por accidente.
        // ═══════════════════════════════════════════════════════════════════════
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // Solo asigna CreatedAt si no fue establecido por el handler
                        var createdAtValue = entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue;
                        if (createdAtValue == null || (DateTime)createdAtValue == default)
                        {
                            entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = utcNow;
                        }
                        break;

                    case EntityState.Modified:
                        // Siempre actualiza UpdatedAt en cualquier modificación
                        entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = utcNow;
                        break;

                    case EntityState.Deleted:
                        // SOFT DELETE: interceptamos el borrado físico
                        // y lo convertimos en una actualización de flags
                        entry.State = EntityState.Modified;
                        entry.Property(nameof(BaseEntity.IsDeleted)).CurrentValue = true;
                        entry.Property(nameof(BaseEntity.DeletedAt)).CurrentValue = utcNow;
                        entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = utcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(ct);
        }
    }
}

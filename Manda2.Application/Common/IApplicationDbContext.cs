using Manda2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Common
{
    public interface IApplicationDbContext
    {
        DbSet<Order> Orders { get; }
        DbSet<SubOrder> SubOrders { get; }
        DbSet<SubOrderDetail> SubOrderDetails { get; }
        DbSet<Driver> Drivers { get; }
        DbSet<CashDeposit> CashDeposits { get; }
        DbSet<AppConfig> AppConfigs { get; }

        // Nuevos DbSets (Fase 4)
        DbSet<Customer> Customers { get; }
        DbSet<Merchant> Merchants { get; }
        DbSet<AuditLog> AuditLogs { get; }

        DbSet<AppUser> AppUsers { get; set; }
        DbSet<ProductCategory> ProductCategories { get; set; }
        DbSet<MerchantCategory> MerchantCategories { get; set; }
        DbSet<PaymentMethod> PaymentMethods { get; set; }
        DbSet<ShippingAddress> ShippingAddresses { get; set; }
        DbSet<CreditNoteType> CreditNoteTypes { get; set; }
        DbSet<CustomerCreditNote> CustomerCreditNotes { get; set; }

        DbSet<Product> Products { get; set; }
        DbSet<MerchantProduct> MerchantProducts { get; set; }

        DbSet<ClosureReason> ClosureReasons { get; set; }

        DbSet<OperatingSchedule> OperatingSchedules { get; set; }
        DbSet<OperatingScheduleException> OperatingScheduleExceptions { get; set; }

        // ─── ÓRDENES MULTI-LOCAL ──────────────────────────────────────────────────
        DbSet<OrderGroup> OrderGroups { get; }
        DbSet<OrderGroupStop> OrderGroupStops { get; }

        // ─── DISPATCH ENGINE ──────────────────────────────────────────────────────
        DbSet<DispatchAttempt> DispatchAttempts { get; }
        DbSet<DispatchAttemptDriver> DispatchAttemptDrivers { get; }
        DbSet<DispatchConfig> DispatchConfigs { get; }

        // Catálogo de motivos de rechazo de ofertas por parte del repartidor.
        // Administrable desde BackOffice sin cambios en código.
        DbSet<DriverRejectionReason> DriverRejectionReasons { get; set; }
        DbSet<OrderGroupPayment> OrderGroupPayments { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        
    }
}

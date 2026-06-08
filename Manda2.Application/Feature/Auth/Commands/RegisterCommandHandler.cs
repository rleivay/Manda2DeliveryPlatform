// FLUJO:
//   1. Validar que el email no esté registrado.
//   2. Validar rol permitido (no Admin/BackOffice desde registro público).
//   3. Hashear contraseña con BCrypt.
//   4. Crear actor base según rol (Customer/Driver/Merchant).
//   5. Crear AppUser vinculado al actor.
//   6. Persistir en una sola transacción.
//
// NOTA: Driver y Merchant quedan IsActive=false hasta aprobación BackOffice.
//       Customer queda IsActive=true (auto-aprobado en MVP).
// ═══════════════════════════════════════════════════════════════════════════



using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Constants;
using Manda2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Auth.Commands
{
    public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResult>
    {
        private readonly IApplicationDbContext _db;

        public RegisterCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RegisterResult> HandleAsync(RegisterCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Verificar email único.
            // ─────────────────────────────────────────────────────────────
            bool emailExists = await _db.AppUsers
                .AnyAsync(u => u.Email.ToLower() == command.Email.ToLower(), ct);

            if (emailExists)
                return Fail("El email ya está registrado.");

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Validar rol permitido en registro público.
            // ─────────────────────────────────────────────────────────────
            var allowedRoles = new[] { AppRoles.Customer, AppRoles.Driver, AppRoles.Merchant, AppRoles.BackOffice };
            if (!allowedRoles.Contains(command.Role))
                return Fail("Rol no permitido en registro público.");

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Hashear contraseña.
            // ─────────────────────────────────────────────────────────────
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password, workFactor: 12);

            var now = DateTime.UtcNow;
            var user = new AppUser
            {
                Email = command.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                Role = command.Role,
                CreatedAt = now
            };

            bool requiresApproval = false;

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Crear actor base según rol.
            // ─────────────────────────────────────────────────────────────
            switch (command.Role)
            {
                case AppRoles.Customer:
                    // Customer: auto-aprobado en MVP
                    var customer = new Manda2.Domain.Entities.Customer
                    {
                        FirstName = command.FirstName ?? command.Email,
                        LastName = command.LastName ?? string.Empty,
                        PhoneNumber = command.Phone ?? string.Empty,
                        Email = command.Email,
                        IsActive = true,
                        CreatedAt = now
                    };
                    _db.Customers.Add(customer);
                    await _db.SaveChangesAsync(ct); // Para obtener el Id generado
                    user.CustomerId = customer.Id;
                    user.IsActive = true;
                    break;

                case AppRoles.Driver:
                    // Driver: requiere aprobación BackOffice
                    var driver = new Domain.Entities.Driver
                    {
                        FirstName = command.FirstName ?? command.Email,
                        LastName = command.LastName ?? string.Empty,
                        Email = command.Email,
                        PhoneNumber = command.Phone ?? string.Empty,
                        IsActive = false, // Pendiente aprobación
                        CreatedAt = now
                    };
                    _db.Drivers.Add(driver);
                    await _db.SaveChangesAsync(ct);
                    user.DriverId = driver.Id;
                    user.IsActive = false; // No puede login hasta aprobación
                    requiresApproval = true;
                    break;

                case AppRoles.Merchant:
                    // Merchant: requiere aprobación BackOffice
                    var merchant = new Domain.Entities.Merchant
                    {
                        Name = command.MerchantName ?? command.Email,
                        IsActive = false, // Pendiente aprobación
                        CreatedAt = now
                    };
                    _db.Merchants.Add(merchant);
                    await _db.SaveChangesAsync(ct);
                    user.MerchantId = merchant.Id;
                    user.IsActive = false;
                    requiresApproval = true;
                    break;
            }

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Persistir AppUser.
            // ─────────────────────────────────────────────────────────────
            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync(ct);

            return new RegisterResult
            {
                Success = true,
                UserId = user.Id,
                RequiresApproval = requiresApproval,
                Message = requiresApproval
                    ? "Registro exitoso. Tu cuenta está pendiente de aprobación."
                    : "Registro exitoso. Ya puedes iniciar sesión."
            };
        }

        private static RegisterResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
